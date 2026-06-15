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

    public async Task<RunningAnalysisCaptureSession> PrepareCaptureSessionAsync(
        RunningAnalysisCaptureSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCaptureSessionRequest(request);

        var captureSession = await _repository.GetCaptureSessionByExternalEventIdAsync(
            request.ExternalEventId.Trim(),
            cancellationToken);

        if (captureSession is null)
        {
            captureSession = new RunningAnalysisCaptureSession
            {
                ExternalEventId = request.ExternalEventId.Trim(),
                CourseId = request.CourseId.Trim(),
                CreatedAt = _clock.UtcNow
            };
        }

        captureSession.CourseId = request.CourseId.Trim();
        captureSession.CourseName = request.CourseName.Trim();
        captureSession.Title = BuildCaptureTitle(request.StartsAt, request.CourseName);
        captureSession.StartsAt = request.StartsAt;
        captureSession.EndsAt = request.EndsAt;
        captureSession.UpdatedAt = _clock.UtcNow;

        if (captureSession.Status == RunningAnalysisEventStatus.Draft)
            captureSession.Status = RunningAnalysisEventStatus.Prepared;

        await _repository.UpsertCaptureSessionAsync(captureSession, cancellationToken);
        return captureSession;
    }

    public async Task<RunningAnalysisEvent> PrepareEventAsync(
        RunningAnalysisEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var captureSession = await PrepareCaptureSessionAsync(
            new RunningAnalysisCaptureSessionRequest(
                request.ExternalEventId,
                request.CourseId,
                request.Title,
                request.StartsAt,
                request.EndsAt),
            cancellationToken);

        return ToLegacyEvent(captureSession);
    }

    public async Task<RunningAnalysisParticipant> RegisterParticipantAsync(
        RunningAnalysisRegistration registration,
        CancellationToken cancellationToken = default)
    {
        ValidateRegistration(registration);

        var courseName = string.IsNullOrWhiteSpace(registration.CourseName)
            ? registration.EventTitle
            : registration.CourseName;

        var captureSession = await PrepareCaptureSessionAsync(
            new RunningAnalysisCaptureSessionRequest(
                registration.ExternalEventId,
                registration.CourseId,
                courseName,
                registration.StartsAt,
                registration.EndsAt),
            cancellationToken);

        var participant = await _repository.GetParticipantAsync(
            captureSession.Id,
            registration.AthleteUserId,
            cancellationToken);

        if (participant is null)
        {
            var participants = await _repository.GetParticipantsAsync(captureSession.Id, cancellationToken);
            participant = new RunningAnalysisParticipant
            {
                CourseId = captureSession.CourseId,
                CaptureSessionId = captureSession.Id,
                AnalysisEventId = captureSession.Id,
                AthleteUserId = registration.AthleteUserId.Trim(),
                SortOrder = participants.Count + 1,
                RegisteredAt = registration.RegisteredAt ?? _clock.UtcNow
            };
        }

        participant.CourseId = captureSession.CourseId;
        participant.CaptureSessionId = captureSession.Id;
        participant.AnalysisEventId = captureSession.Id;
        participant.DisplayName = Normalize(registration.DisplayName, registration.AthleteUserId);
        participant.Email = registration.Email.Trim();
        participant.CreatedFromRegistrationId = registration.RegistrationId;
        await ProvisionParticipantFolderAsync(captureSession, participant, cancellationToken);
        await _repository.UpsertParticipantAsync(participant, cancellationToken);

        return participant;
    }

    public async Task<IReadOnlyList<RunningAnalysisRosterItem>> GetRosterAsync(
        string captureSessionId,
        CancellationToken cancellationToken = default)
    {
        var participants = await _repository.GetParticipantsAsync(captureSessionId, cancellationToken);
        var result = new List<RunningAnalysisRosterItem>();

        foreach (var participant in participants.OrderBy(participant => participant.SortOrder).ThenBy(participant => participant.DisplayName))
        {
            var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);
            var primaryRecordingUrl = recordings.FirstOrDefault(recording => recording.IsPrimary)?.DriveFileUrl;
            result.Add(new RunningAnalysisRosterItem(
                participant.Id,
                participant.AthleteUserId,
                participant.Email,
                participant.DisplayName,
                participant.SortOrder,
                participant.FolderStatus,
                participant.PermissionStatus,
                recordings.Count,
                primaryRecordingUrl));
        }

        return result;
    }

    public async Task<IReadOnlyList<RunningAnalysisCaptureLink>> GetCapturesForAthleteAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<RunningAnalysisCaptureLink>();

        var participants = await _repository.GetParticipantsForAthleteAsync(athleteUserId, cancellationToken);
        var result = new List<RunningAnalysisCaptureLink>();

        foreach (var participant in participants.Where(participant => !participant.IsHiddenFromAthlete))
        {
            var captureSessionId = GetParticipantCaptureSessionId(participant);
            var captureSession = await _repository.GetCaptureSessionAsync(captureSessionId, cancellationToken);
            if (captureSession is null)
                continue;

            result.Add(new RunningAnalysisCaptureLink(
                captureSession.Id,
                captureSession.ExternalEventId,
                captureSession.Title,
                captureSession.StartsAt,
                captureSession.Status,
                participant.FolderStatus,
                participant.PermissionStatus,
                participant.DriveFolderUrl));
        }

        return result
            .OrderByDescending(capture => capture.StartsAt)
            .ThenBy(capture => capture.Title)
            .ToList();
    }

    public async Task<IReadOnlyList<RunningAnalysisLink>> GetAnalysesForAthleteAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        var captures = await GetCapturesForAthleteAsync(athleteUserId, cancellationToken);
        return captures
            .Select(capture => new RunningAnalysisLink(
                capture.CaptureSessionId,
                capture.ExternalEventId,
                capture.Title,
                capture.StartsAt,
                capture.CaptureStatus,
                capture.FolderStatus,
                capture.PermissionStatus,
                capture.DriveFolderUrl))
            .ToList();
    }

    public async Task HideCaptureForAthleteAsync(
        string athleteUserId,
        string captureSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException("The athlete user id is required.");

        if (string.IsNullOrWhiteSpace(captureSessionId))
            throw new InvalidOperationException("The running capture id is required.");

        var participant = await _repository.GetParticipantAsync(
            captureSessionId.Trim(),
            athleteUserId.Trim(),
            cancellationToken) ?? throw new InvalidOperationException("The running capture was not found for this athlete.");

        participant.IsHiddenFromAthlete = true;
        await _repository.UpsertParticipantAsync(participant, cancellationToken);
    }

    public Task HideAnalysisForAthleteAsync(
        string athleteUserId,
        string analysisEventId,
        CancellationToken cancellationToken = default)
    {
        return HideCaptureForAthleteAsync(athleteUserId, analysisEventId, cancellationToken);
    }

    public async Task<RunningAnalysisCaptureSession> StartCaptureAsync(
        string captureSessionId,
        CancellationToken cancellationToken = default)
    {
        var captureSession = await RequireCaptureSessionAsync(captureSessionId, cancellationToken);

        captureSession.Status = RunningAnalysisEventStatus.InProgress;
        captureSession.UpdatedAt = _clock.UtcNow;
        await _repository.UpsertCaptureSessionAsync(captureSession, cancellationToken);
        return captureSession;
    }

    public async Task<RunningAnalysisEvent> StartAnalysisAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default)
    {
        return ToLegacyEvent(await StartCaptureAsync(analysisEventId, cancellationToken));
    }

    public async Task<RunningAnalysisCaptureSession> CompleteCaptureAsync(
        string captureSessionId,
        CancellationToken cancellationToken = default)
    {
        var captureSession = await RequireCaptureSessionAsync(captureSessionId, cancellationToken);
        captureSession.Status = RunningAnalysisEventStatus.Completed;
        captureSession.UpdatedAt = _clock.UtcNow;
        await _repository.UpsertCaptureSessionAsync(captureSession, cancellationToken);
        return captureSession;
    }

    public async Task<RunningAnalysisEvent> CompleteAnalysisAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default)
    {
        return ToLegacyEvent(await CompleteCaptureAsync(analysisEventId, cancellationToken));
    }

    public async Task<RunningAnalysisRecording> UploadRecordingAsync(
        string captureSessionId,
        string participantId,
        string fileName,
        string contentType,
        Stream content,
        bool isOnline,
        CancellationToken cancellationToken = default)
    {
        if (!isOnline)
            throw new InvalidOperationException("Running capture recordings can only be uploaded while online.");

        if (content is null)
            throw new ArgumentNullException(nameof(content));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("A recording file name is required.");

        var captureSession = await RequireCaptureSessionAsync(captureSessionId, cancellationToken);
        var participant = await _repository.GetParticipantByIdAsync(participantId, cancellationToken)
            ?? throw new InvalidOperationException("The running capture participant was not found.");

        if (GetParticipantCaptureSessionId(participant) != captureSession.Id)
            throw new InvalidOperationException("The participant does not belong to this running capture.");

        await EnsureParticipantPersonalFolderAsync(captureSession, participant, cancellationToken);
        await _repository.UpsertParticipantAsync(participant, cancellationToken);

        if (participant.FolderStatus != RunningAnalysisFolderStatus.Ready || string.IsNullOrWhiteSpace(participant.DriveFolderId))
            throw new InvalidOperationException("The participant folder is not ready for uploads.");

        var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);
        var recording = new RunningAnalysisRecording
        {
            CourseId = captureSession.CourseId,
            CaptureSessionId = captureSession.Id,
            AnalysisEventId = captureSession.Id,
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
                new UploadRecordingRequest(ToLegacyEvent(captureSession), participant, recording, content),
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
        RunningAnalysisCaptureSession captureSession,
        RunningAnalysisParticipant participant,
        CancellationToken cancellationToken)
    {
        participant.FolderStatus = RunningAnalysisFolderStatus.Creating;
        participant.PermissionStatus = RunningAnalysisPermissionStatus.Missing;
        participant.ProvisioningError = null;

        try
        {
            await EnsureParticipantPersonalFolderAsync(captureSession, participant, cancellationToken);
        }
        catch (Exception ex)
        {
            if (participant.FolderStatus != RunningAnalysisFolderStatus.Ready)
                participant.FolderStatus = RunningAnalysisFolderStatus.Failed;

            participant.PermissionStatus = RunningAnalysisPermissionStatus.Failed;
            participant.ProvisioningError = ex.Message;
        }
    }

    private async Task EnsureParticipantPersonalFolderAsync(
        RunningAnalysisCaptureSession captureSession,
        RunningAnalysisParticipant participant,
        CancellationToken cancellationToken)
    {
        var participantFolder = await _userDriveFolderService.GetFolderAsync(
            participant.AthleteUserId,
            cancellationToken);

        participantFolder ??= await _userDriveFolderService.CreateFolderAsync(
            new UserDriveFolderRequest(participant.AthleteUserId, participant.Email),
            cancellationToken);

        await _folderRegistry.SaveFolderReferenceAsync(
            new SaveDriveFolderReferenceRequest(
                captureSession.CourseId,
                captureSession.ExternalEventId,
                participant.AthleteUserId,
                participant.Email,
                participantFolder.FolderId,
                participantFolder.Url),
            cancellationToken);

        participant.CaptureSessionId = captureSession.Id;
        participant.AnalysisEventId = captureSession.Id;
        participant.DriveFolderId = participantFolder.FolderId;
        participant.DriveFolderUrl = participantFolder.Url;
        participant.FolderStatus = RunningAnalysisFolderStatus.Ready;
        participant.PermissionStatus = RunningAnalysisPermissionStatus.Granting;
        participant.ProvisioningError = null;

        if (string.IsNullOrWhiteSpace(participant.Email))
            throw new InvalidOperationException("A participant email is required to grant write access.");

        await _storageProvider.GrantParticipantWriteAccessAsync(participantFolder, participant.Email, cancellationToken);
        participant.PermissionStatus = RunningAnalysisPermissionStatus.Granted;
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

    private async Task<RunningAnalysisCaptureSession> RequireCaptureSessionAsync(
        string captureSessionId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(captureSessionId))
            throw new InvalidOperationException("A running capture session id is required.");

        return await _repository.GetCaptureSessionAsync(captureSessionId, cancellationToken)
            ?? throw new InvalidOperationException("The running capture session was not found.");
    }

    private static void ValidateRegistration(RunningAnalysisRegistration registration)
    {
        if (string.IsNullOrWhiteSpace(registration.ExternalEventId))
            throw new InvalidOperationException("A source event id is required.");

        if (string.IsNullOrWhiteSpace(registration.CourseId))
            throw new InvalidOperationException("A course id is required.");

        if (string.IsNullOrWhiteSpace(registration.EventTitle)
            && string.IsNullOrWhiteSpace(registration.CourseName))
        {
            throw new InvalidOperationException("A running capture title source is required.");
        }

        if (registration.EndsAt <= registration.StartsAt)
            throw new InvalidOperationException("The running capture end must be after the start.");

        if (string.IsNullOrWhiteSpace(registration.AthleteUserId))
            throw new InvalidOperationException("An athlete user id is required.");
    }

    private static void ValidateCaptureSessionRequest(RunningAnalysisCaptureSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalEventId))
            throw new InvalidOperationException("A source event id is required.");

        if (string.IsNullOrWhiteSpace(request.CourseId))
            throw new InvalidOperationException("A course id is required.");

        if (string.IsNullOrWhiteSpace(request.CourseName))
            throw new InvalidOperationException("A course name is required.");

        if (request.EndsAt <= request.StartsAt)
            throw new InvalidOperationException("The running capture end must be after the start.");
    }

    private static string BuildCaptureTitle(DateTime startsAt, string courseName)
    {
        return $"{startsAt:yyyy-MM-dd} {courseName.Trim()}";
    }

    private static string GetParticipantCaptureSessionId(RunningAnalysisParticipant participant)
    {
        return string.IsNullOrWhiteSpace(participant.CaptureSessionId)
            ? participant.AnalysisEventId
            : participant.CaptureSessionId;
    }

    private static RunningAnalysisEvent ToLegacyEvent(RunningAnalysisCaptureSession captureSession)
    {
        return new RunningAnalysisEvent
        {
            Id = captureSession.Id,
            DocumentType = captureSession.DocumentType,
            ExternalEventId = captureSession.ExternalEventId,
            CourseId = captureSession.CourseId,
            Title = captureSession.Title,
            StartsAt = captureSession.StartsAt,
            EndsAt = captureSession.EndsAt,
            Status = captureSession.Status,
            DriveFolderId = captureSession.DriveFolderId,
            DriveFolderUrl = captureSession.DriveFolderUrl,
            CreatedAt = captureSession.CreatedAt,
            UpdatedAt = captureSession.UpdatedAt
        };
    }

    private static string Normalize(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback.Trim()
            : value.Trim();
    }
}
