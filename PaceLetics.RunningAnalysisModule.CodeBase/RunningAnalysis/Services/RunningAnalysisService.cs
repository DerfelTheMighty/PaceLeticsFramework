using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;

public sealed class RunningAnalysisService : IRunningAnalysisService
{
    private readonly IRunningAnalysisRepository _repository;
    private readonly IRunningAnalysisStorageProvider _storageProvider;
    private readonly IUserDriveFolderRegistry _folderRegistry;
    private readonly IUserDriveFolderService _userDriveFolderService;
    private readonly IRunningAnalysisClock _clock;

    public RunningAnalysisService(
        IRunningAnalysisRepository repository,
        IRunningAnalysisStorageProvider storageProvider,
        IUserDriveFolderRegistry folderRegistry,
        IUserDriveFolderService userDriveFolderService,
        IRunningAnalysisClock clock)
    {
        _repository = repository;
        _storageProvider = storageProvider;
        _folderRegistry = folderRegistry;
        _userDriveFolderService = userDriveFolderService;
        _clock = clock;
    }

    public async Task<RunningAnalysisEvent> PrepareEventAsync(
        RunningAnalysisEventRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateEventRequest(request);

        var analysisEvent = await _repository.GetEventByExternalEventIdAsync(
            request.ExternalEventId,
            cancellationToken);

        if (analysisEvent is null)
        {
            analysisEvent = new RunningAnalysisEvent
            {
                ExternalEventId = request.ExternalEventId.Trim(),
                CourseId = request.CourseId.Trim(),
                CreatedAt = _clock.UtcNow
            };
        }

        analysisEvent.CourseId = request.CourseId.Trim();
        analysisEvent.Title = request.Title.Trim();
        analysisEvent.StartsAt = request.StartsAt;
        analysisEvent.EndsAt = request.EndsAt;
        analysisEvent.UpdatedAt = _clock.UtcNow;

        if (analysisEvent.Status == RunningAnalysisEventStatus.Draft)
            analysisEvent.Status = RunningAnalysisEventStatus.Prepared;

        await _repository.UpsertEventAsync(analysisEvent, cancellationToken);
        return analysisEvent;
    }

    public async Task<RunningAnalysisParticipant> RegisterParticipantAsync(
        RunningAnalysisRegistration registration,
        CancellationToken cancellationToken = default)
    {
        ValidateRegistration(registration);

        var analysisEvent = await PrepareEventAsync(
            new RunningAnalysisEventRequest(
                registration.ExternalEventId,
                registration.CourseId,
                registration.EventTitle,
                registration.StartsAt,
                registration.EndsAt),
            cancellationToken);
        var participant = await _repository.GetParticipantAsync(
            analysisEvent.Id,
            registration.AthleteUserId,
            cancellationToken);

        if (participant is null)
        {
            var participants = await _repository.GetParticipantsAsync(analysisEvent.Id, cancellationToken);
            participant = new RunningAnalysisParticipant
            {
                CourseId = analysisEvent.CourseId,
                AnalysisEventId = analysisEvent.Id,
                AthleteUserId = registration.AthleteUserId.Trim(),
                SortOrder = participants.Count + 1,
                RegisteredAt = registration.RegisteredAt ?? _clock.UtcNow
            };
        }

        participant.CourseId = analysisEvent.CourseId;
        participant.DisplayName = Normalize(registration.DisplayName, registration.AthleteUserId);
        participant.Email = registration.Email.Trim();
        participant.CreatedFromRegistrationId = registration.RegistrationId;
        await ProvisionParticipantFolderAsync(analysisEvent, participant, cancellationToken);
        await _repository.UpsertParticipantAsync(participant, cancellationToken);

        return participant;
    }

    public async Task<IReadOnlyList<RunningAnalysisRosterItem>> GetRosterAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default)
    {
        var participants = await _repository.GetParticipantsAsync(analysisEventId, cancellationToken);
        var result = new List<RunningAnalysisRosterItem>();

        foreach (var participant in participants.OrderBy(participant => participant.SortOrder).ThenBy(participant => participant.DisplayName))
        {
            var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);
            var primaryRecordingUrl = recordings.FirstOrDefault(recording => recording.IsPrimary)?.DriveFileUrl;
            result.Add(new RunningAnalysisRosterItem(
                participant.Id,
                participant.AthleteUserId,
                participant.DisplayName,
                participant.SortOrder,
                participant.FolderStatus,
                participant.PermissionStatus,
                recordings.Count,
                primaryRecordingUrl));
        }

        return result;
    }

    public async Task<IReadOnlyList<RunningAnalysisLink>> GetAnalysesForAthleteAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<RunningAnalysisLink>();

        var participants = await _repository.GetParticipantsForAthleteAsync(athleteUserId, cancellationToken);
        var result = new List<RunningAnalysisLink>();

        foreach (var participant in participants)
        {
            var analysisEvent = await _repository.GetEventAsync(participant.AnalysisEventId, cancellationToken);
            if (analysisEvent is null)
                continue;

            result.Add(new RunningAnalysisLink(
                analysisEvent.Id,
                analysisEvent.ExternalEventId,
                analysisEvent.Title,
                analysisEvent.StartsAt,
                analysisEvent.Status,
                participant.FolderStatus,
                participant.PermissionStatus,
                participant.DriveFolderUrl));
        }

        return result
            .OrderByDescending(analysis => analysis.StartsAt)
            .ThenBy(analysis => analysis.Title)
            .ToList();
    }

    public async Task<RunningAnalysisEvent> StartAnalysisAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default)
    {
        var analysisEvent = await RequireEventAsync(analysisEventId, cancellationToken);
        if (analysisEvent.Status == RunningAnalysisEventStatus.Completed)
            throw new InvalidOperationException("A completed running analysis cannot be restarted.");

        analysisEvent.Status = RunningAnalysisEventStatus.InProgress;
        analysisEvent.UpdatedAt = _clock.UtcNow;
        await _repository.UpsertEventAsync(analysisEvent, cancellationToken);
        return analysisEvent;
    }

    public async Task<RunningAnalysisEvent> CompleteAnalysisAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default)
    {
        var analysisEvent = await RequireEventAsync(analysisEventId, cancellationToken);
        analysisEvent.Status = RunningAnalysisEventStatus.Completed;
        analysisEvent.UpdatedAt = _clock.UtcNow;
        await _repository.UpsertEventAsync(analysisEvent, cancellationToken);
        return analysisEvent;
    }

    public async Task<RunningAnalysisRecording> UploadRecordingAsync(
        string analysisEventId,
        string participantId,
        string fileName,
        string contentType,
        Stream content,
        bool isOnline,
        CancellationToken cancellationToken = default)
    {
        if (!isOnline)
            throw new InvalidOperationException("Running analysis recordings can only be uploaded while online.");

        if (content is null)
            throw new ArgumentNullException(nameof(content));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("A recording file name is required.");

        var analysisEvent = await RequireEventAsync(analysisEventId, cancellationToken);
        var participant = await _repository.GetParticipantByIdAsync(participantId, cancellationToken)
            ?? throw new InvalidOperationException("The running analysis participant was not found.");

        if (participant.AnalysisEventId != analysisEvent.Id)
            throw new InvalidOperationException("The participant does not belong to this running analysis.");

        if (participant.FolderStatus != RunningAnalysisFolderStatus.Ready || string.IsNullOrWhiteSpace(participant.DriveFolderId))
            throw new InvalidOperationException("The participant folder is not ready for uploads.");

        var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);
        var recording = new RunningAnalysisRecording
        {
            CourseId = analysisEvent.CourseId,
            AnalysisEventId = analysisEvent.Id,
            ParticipantId = participant.Id,
            AttemptNumber = recordings.Count + 1,
            RecordedAt = _clock.UtcNow,
            FileName = fileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "video/webm" : contentType.Trim(),
            UploadStatus = RunningAnalysisUploadStatus.Uploading
        };

        await _repository.UpsertRecordingAsync(recording, cancellationToken);

        try
        {
            var driveFile = await _storageProvider.UploadRecordingAsync(
                new UploadRecordingRequest(analysisEvent, participant, recording, content),
                cancellationToken);

            recording.DriveFileId = driveFile.FileId;
            recording.DriveFileUrl = driveFile.Url;
            recording.UploadStatus = RunningAnalysisUploadStatus.Uploaded;
            recording.ErrorMessage = null;
            await SetPrimaryRecordingAsync(participant.Id, recording, cancellationToken);
            return recording;
        }
        catch (Exception ex)
        {
            recording.UploadStatus = RunningAnalysisUploadStatus.Failed;
            recording.ErrorMessage = ex.Message;
            recording.IsPrimary = false;
            await _repository.UpsertRecordingAsync(recording, cancellationToken);
            return recording;
        }
    }

    private async Task ProvisionParticipantFolderAsync(
        RunningAnalysisEvent analysisEvent,
        RunningAnalysisParticipant participant,
        CancellationToken cancellationToken)
    {
        participant.FolderStatus = RunningAnalysisFolderStatus.Creating;
        participant.PermissionStatus = RunningAnalysisPermissionStatus.Missing;
        participant.ProvisioningError = null;

        try
        {
            var participantFolder = await _userDriveFolderService.GetFolderAsync(
                participant.AthleteUserId,
                cancellationToken);

            participantFolder ??= await _userDriveFolderService.CreateFolderAsync(
                new UserDriveFolderRequest(participant.AthleteUserId, participant.Email),
                cancellationToken);

            await _folderRegistry.SaveFolderReferenceAsync(
                new SaveDriveFolderReferenceRequest(
                    analysisEvent.CourseId,
                    analysisEvent.ExternalEventId,
                    participant.AthleteUserId,
                    participant.Email,
                    participantFolder.FolderId,
                    participantFolder.Url),
                cancellationToken);

            participant.DriveFolderId = participantFolder.FolderId;
            participant.DriveFolderUrl = participantFolder.Url;
            participant.FolderStatus = RunningAnalysisFolderStatus.Ready;
            participant.PermissionStatus = RunningAnalysisPermissionStatus.Granting;

            if (string.IsNullOrWhiteSpace(participant.Email))
                throw new InvalidOperationException("A participant email is required to grant write access.");

            await _storageProvider.GrantParticipantWriteAccessAsync(participantFolder, participant.Email, cancellationToken);
            participant.PermissionStatus = RunningAnalysisPermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            if (participant.FolderStatus != RunningAnalysisFolderStatus.Ready)
                participant.FolderStatus = RunningAnalysisFolderStatus.Failed;

            participant.PermissionStatus = RunningAnalysisPermissionStatus.Failed;
            participant.ProvisioningError = ex.Message;
        }
    }

    private async Task SetPrimaryRecordingAsync(
        string participantId,
        RunningAnalysisRecording primaryRecording,
        CancellationToken cancellationToken)
    {
        var recordings = await _repository.GetRecordingsForParticipantAsync(participantId, cancellationToken);
        foreach (var recording in recordings)
        {
            recording.IsPrimary = recording.Id == primaryRecording.Id;
            await _repository.UpsertRecordingAsync(recording, cancellationToken);
        }
    }

    private async Task<RunningAnalysisEvent> RequireEventAsync(
        string analysisEventId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(analysisEventId))
            throw new InvalidOperationException("A running analysis event id is required.");

        return await _repository.GetEventAsync(analysisEventId, cancellationToken)
            ?? throw new InvalidOperationException("The running analysis event was not found.");
    }

    private static void ValidateRegistration(RunningAnalysisRegistration registration)
    {
        if (string.IsNullOrWhiteSpace(registration.ExternalEventId))
            throw new InvalidOperationException("A source event id is required.");

        if (string.IsNullOrWhiteSpace(registration.CourseId))
            throw new InvalidOperationException("A course id is required.");

        if (string.IsNullOrWhiteSpace(registration.EventTitle))
            throw new InvalidOperationException("A running analysis title is required.");

        if (registration.EndsAt <= registration.StartsAt)
            throw new InvalidOperationException("The running analysis end must be after the start.");

        if (string.IsNullOrWhiteSpace(registration.AthleteUserId))
            throw new InvalidOperationException("An athlete user id is required.");
    }

    private static void ValidateEventRequest(RunningAnalysisEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalEventId))
            throw new InvalidOperationException("A source event id is required.");

        if (string.IsNullOrWhiteSpace(request.CourseId))
            throw new InvalidOperationException("A course id is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidOperationException("A running analysis title is required.");

        if (request.EndsAt <= request.StartsAt)
            throw new InvalidOperationException("The running analysis end must be after the start.");
    }

    private static string Normalize(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback.Trim()
            : value.Trim();
    }
}
