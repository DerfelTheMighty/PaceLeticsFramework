using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;

public sealed class RunningAnalysisService : IRunningAnalysisService
{
    private static readonly RunningAnalysisCriterion[] SideCriteria =
    [
        RunningAnalysisCriterion.Overstride,
        RunningAnalysisCriterion.TrunkPosture,
        RunningAnalysisCriterion.HipExtension,
        RunningAnalysisCriterion.ArmSwingSide
    ];

    private static readonly RunningAnalysisCriterion[] RearCriteria =
    [
        RunningAnalysisCriterion.InternalRotation,
        RunningAnalysisCriterion.PelvicDrop,
        RunningAnalysisCriterion.ArmSwingRear
    ];

    private static readonly JsonSerializerOptions ResultJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

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
            var analysisResult = await _repository.GetResultForParticipantAsync(captureSessionId, participant.Id, cancellationToken);
            result.Add(new RunningAnalysisRosterItem(
                participant.Id,
                participant.AthleteUserId,
                participant.Email,
                participant.DisplayName,
                participant.SortOrder,
                participant.FolderStatus,
                participant.PermissionStatus,
                recordings.Count,
                primaryRecordingUrl,
                participant.DriveFolderUrl,
                GetLatestUploadedRecordingUrl(recordings, RunningAnalysisPerspective.Side),
                GetLatestUploadedRecordingUrl(recordings, RunningAnalysisPerspective.Rear),
                analysisResult?.Status,
                analysisResult?.TotalScore,
                analysisResult?.TotalMaxScore));
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
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<RunningAnalysisLink>();

        var participants = await _repository.GetParticipantsForAthleteAsync(athleteUserId.Trim(), cancellationToken);
        var links = new List<RunningAnalysisLink>();

        foreach (var participant in participants.Where(participant => !participant.IsHiddenFromAthlete))
        {
            var captureSessionId = GetParticipantCaptureSessionId(participant);
            var captureSession = await _repository.GetCaptureSessionAsync(captureSessionId, cancellationToken);
            if (captureSession is null)
                continue;

            var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);
            var analysisResult = await _repository.GetResultForParticipantAsync(captureSession.Id, participant.Id, cancellationToken);
            if (analysisResult?.Status != RunningAnalysisResultStatus.Completed)
                analysisResult = null;

            links.Add(new RunningAnalysisLink(
                captureSession.Id,
                captureSession.ExternalEventId,
                captureSession.Title,
                captureSession.StartsAt,
                captureSession.Status,
                participant.FolderStatus,
                participant.PermissionStatus,
                participant.DriveFolderUrl,
                analysisResult?.Id,
                analysisResult?.Status,
                analysisResult?.AnalyzedAt,
                analysisResult?.Summary ?? string.Empty,
                analysisResult?.SideScore,
                analysisResult?.SideMaxScore,
                analysisResult?.RearScore,
                analysisResult?.RearMaxScore,
                analysisResult?.TotalScore,
                analysisResult?.TotalMaxScore,
                analysisResult?.SideRecordingUrl ?? GetLatestUploadedRecordingUrl(recordings, RunningAnalysisPerspective.Side),
                analysisResult?.RearRecordingUrl ?? GetLatestUploadedRecordingUrl(recordings, RunningAnalysisPerspective.Rear),
                analysisResult?.ResultDriveFileUrl,
                analysisResult?.SideAssessment,
                analysisResult?.RearAssessment));
        }

        return links
            .OrderByDescending(link => link.AnalyzedAt ?? link.StartsAt)
            .ThenBy(link => link.Title)
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

    public async Task<RunningAnalysisResult?> GetAnalysisResultForParticipantAsync(
        string captureSessionId,
        string participantId,
        CancellationToken cancellationToken = default)
    {
        var captureSession = await RequireCaptureSessionAsync(captureSessionId, cancellationToken);
        var participant = await _repository.GetParticipantByIdAsync(participantId, cancellationToken)
            ?? throw new InvalidOperationException("The running capture participant was not found.");

        if (GetParticipantCaptureSessionId(participant) != captureSession.Id)
            throw new InvalidOperationException("The participant does not belong to this running capture.");

        return await _repository.GetResultForParticipantAsync(captureSession.Id, participant.Id, cancellationToken);
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
        RunningAnalysisPerspective perspective = RunningAnalysisPerspective.Side,
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
            Perspective = NormalizePerspective(perspective),
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

    public async Task<RunningAnalysisRecording> RegisterUploadedRecordingAsync(
        string captureSessionId,
        string participantId,
        string fileName,
        string contentType,
        DriveFileReference driveFile,
        RunningAnalysisPerspective perspective = RunningAnalysisPerspective.Side,
        DateTime? recordedAt = null,
        CancellationToken cancellationToken = default)
    {
        if (driveFile is null)
            throw new ArgumentNullException(nameof(driveFile));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("A recording file name is required.");

        var captureSession = await RequireCaptureSessionAsync(captureSessionId, cancellationToken);
        var participant = await _repository.GetParticipantByIdAsync(participantId, cancellationToken)
            ?? throw new InvalidOperationException("The running capture participant was not found.");

        if (GetParticipantCaptureSessionId(participant) != captureSession.Id)
            throw new InvalidOperationException("The participant does not belong to this running capture.");

        var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);
        var recording = recordings.FirstOrDefault(recording =>
            !string.IsNullOrWhiteSpace(driveFile.FileId)
            && string.Equals(recording.DriveFileId, driveFile.FileId, StringComparison.Ordinal));

        recording ??= new RunningAnalysisRecording
        {
            CourseId = captureSession.CourseId,
            CaptureSessionId = captureSession.Id,
            AnalysisEventId = captureSession.Id,
            ParticipantId = participant.Id,
            AttemptNumber = recordings.Count + 1
        };

        recording.CourseId = captureSession.CourseId;
        recording.CaptureSessionId = captureSession.Id;
        recording.AnalysisEventId = captureSession.Id;
        recording.ParticipantId = participant.Id;
        recording.Perspective = NormalizePerspective(perspective);
        recording.RecordedAt = recordedAt ?? _clock.UtcNow;
        recording.FileName = fileName.Trim();
        recording.ContentType = string.IsNullOrWhiteSpace(contentType) ? "video/webm" : contentType.Trim();
        recording.DriveFileId = driveFile.FileId;
        recording.DriveFileUrl = driveFile.Url;
        recording.UploadStatus = RunningAnalysisUploadStatus.Uploaded;
        recording.ErrorMessage = null;

        await _repository.UpsertRecordingAsync(recording, cancellationToken);
        await SetPrimaryRecordingAsync(participant.Id, recording, cancellationToken);
        return recording;
    }

    public async Task<RunningAnalysisResult> SaveAnalysisResultAsync(
        RunningAnalysisResultRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateResultRequest(request);

        var captureSession = await RequireCaptureSessionAsync(request.CaptureSessionId, cancellationToken);
        var participant = await _repository.GetParticipantByIdAsync(request.ParticipantId, cancellationToken)
            ?? throw new InvalidOperationException("The running capture participant was not found.");

        if (GetParticipantCaptureSessionId(participant) != captureSession.Id)
            throw new InvalidOperationException("The participant does not belong to this running capture.");

        var result = await _repository.GetResultForParticipantAsync(captureSession.Id, participant.Id, cancellationToken)
            ?? new RunningAnalysisResult
            {
                CaptureSessionId = captureSession.Id,
                CourseId = captureSession.CourseId,
                ExternalEventId = captureSession.ExternalEventId,
                ParticipantId = participant.Id,
                AthleteUserId = participant.AthleteUserId,
                CreatedAt = _clock.UtcNow
            };

        var sideAssessment = NormalizeAssessmentItems(
            request.SideAssessment,
            RunningAnalysisPerspective.Side,
            SideCriteria);
        var rearAssessment = NormalizeAssessmentItems(
            request.RearAssessment,
            RunningAnalysisPerspective.Rear,
            RearCriteria);
        var recordings = await _repository.GetRecordingsForParticipantAsync(participant.Id, cancellationToken);

        result.CaptureSessionId = captureSession.Id;
        result.CourseId = captureSession.CourseId;
        result.ExternalEventId = captureSession.ExternalEventId;
        result.ParticipantId = participant.Id;
        result.AthleteUserId = participant.AthleteUserId;
        result.AthleteDisplayName = participant.DisplayName;
        result.AnalyzerTrainerUserId = request.TrainerUserId.Trim();
        result.AnalyzerDisplayName = Normalize(request.TrainerDisplayName, request.TrainerUserId);
        result.Title = captureSession.Title;
        result.Status = request.Complete ? RunningAnalysisResultStatus.Completed : RunningAnalysisResultStatus.Draft;
        result.SideAssessment = sideAssessment;
        result.RearAssessment = rearAssessment;
        result.Summary = request.Summary.Trim();
        result.SideScore = CalculateScore(sideAssessment);
        result.SideMaxScore = CalculateMaxScore(sideAssessment);
        result.RearScore = CalculateScore(rearAssessment);
        result.RearMaxScore = CalculateMaxScore(rearAssessment);
        result.TotalScore = result.SideScore + result.RearScore;
        result.TotalMaxScore = result.SideMaxScore + result.RearMaxScore;
        result.SideRecordingUrl = GetLatestUploadedRecordingUrl(recordings, RunningAnalysisPerspective.Side);
        result.RearRecordingUrl = GetLatestUploadedRecordingUrl(recordings, RunningAnalysisPerspective.Rear);
        result.UpdatedAt = _clock.UtcNow;

        if (request.Complete)
        {
            result.AnalyzedAt ??= _clock.UtcNow;
            result.PublishedAt = _clock.UtcNow;
            var driveFile = await UploadResultDocumentAsync(captureSession, participant, result, cancellationToken);
            result.ResultDriveFileId = driveFile.FileId;
            result.ResultDriveFileUrl = driveFile.Url;
        }
        else
        {
            result.PublishedAt = null;
        }

        await _repository.UpsertResultAsync(result, cancellationToken);
        return result;
    }

    private async Task<DriveFileReference> UploadResultDocumentAsync(
        RunningAnalysisCaptureSession captureSession,
        RunningAnalysisParticipant participant,
        RunningAnalysisResult result,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(result, ResultJsonOptions);
        await using var content = new MemoryStream(json);

        return await _userDriveFolderService.UploadAnalysisResultAsync(
            new UserDriveAnalysisResultUploadRequest(
                participant.AthleteUserId,
                participant.Email,
                captureSession.Title,
                captureSession.StartsAt,
                BuildResultFileName(captureSession, participant),
                "application/json",
                content),
            cancellationToken);
    }

    private static List<RunningAnalysisAssessmentItem> NormalizeAssessmentItems(
        IEnumerable<RunningAnalysisAssessmentItem> items,
        RunningAnalysisPerspective perspective,
        IReadOnlyCollection<RunningAnalysisCriterion> expectedCriteria)
    {
        var byCriterion = items
            .GroupBy(item => item.Criterion)
            .ToDictionary(group => group.Key, group => group.Last());

        return expectedCriteria
            .Select(criterion =>
            {
                byCriterion.TryGetValue(criterion, out var item);
                return new RunningAnalysisAssessmentItem
                {
                    Criterion = criterion,
                    Perspective = perspective,
                    Rating = NormalizeRating(item?.Rating ?? RunningAnalysisAssessmentRating.NotAssessable),
                    Confidence = NormalizeConfidence(item?.Confidence ?? RunningAnalysisAssessmentConfidence.Medium),
                    Notes = item?.Notes?.Trim() ?? string.Empty
                };
            })
            .ToList();
    }

    private static int CalculateScore(IEnumerable<RunningAnalysisAssessmentItem> items)
    {
        return items
            .Where(item => item.Rating >= RunningAnalysisAssessmentRating.Normal)
            .Sum(item => (int)item.Rating);
    }

    private static int CalculateMaxScore(IEnumerable<RunningAnalysisAssessmentItem> items)
    {
        return items.Count(item => item.Rating >= RunningAnalysisAssessmentRating.Normal) * 2;
    }

    private static string? GetLatestUploadedRecordingUrl(
        IEnumerable<RunningAnalysisRecording> recordings,
        RunningAnalysisPerspective perspective)
    {
        return recordings
            .Where(recording =>
                recording.UploadStatus == RunningAnalysisUploadStatus.Uploaded
                && recording.Perspective == perspective
                && !string.IsNullOrWhiteSpace(recording.DriveFileUrl))
            .OrderByDescending(recording => recording.RecordedAt)
            .ThenByDescending(recording => recording.AttemptNumber)
            .Select(recording => recording.DriveFileUrl)
            .FirstOrDefault();
    }

    private static string BuildResultFileName(
        RunningAnalysisCaptureSession captureSession,
        RunningAnalysisParticipant participant)
    {
        var athleteName = SanitizeFileName(participant.DisplayName);
        return $"{captureSession.StartsAt:yyyyMMdd}-{athleteName}-laufanalyse-ergebnis.json";
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string((value ?? string.Empty)
            .Trim()
            .Select(character => invalid.Contains(character) || char.IsWhiteSpace(character) ? '-' : character)
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized)
            ? "athlet"
            : sanitized;
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

    private static void ValidateResultRequest(RunningAnalysisResultRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CaptureSessionId))
            throw new InvalidOperationException("A running capture session id is required.");

        if (string.IsNullOrWhiteSpace(request.ParticipantId))
            throw new InvalidOperationException("A running capture participant id is required.");

        if (string.IsNullOrWhiteSpace(request.TrainerUserId))
            throw new InvalidOperationException("A trainer user id is required.");
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

    private static RunningAnalysisPerspective NormalizePerspective(RunningAnalysisPerspective perspective)
    {
        return perspective == RunningAnalysisPerspective.Rear
            ? RunningAnalysisPerspective.Rear
            : RunningAnalysisPerspective.Side;
    }

    private static RunningAnalysisAssessmentRating NormalizeRating(RunningAnalysisAssessmentRating rating)
    {
        return Enum.IsDefined(typeof(RunningAnalysisAssessmentRating), rating)
            ? rating
            : RunningAnalysisAssessmentRating.NotAssessable;
    }

    private static RunningAnalysisAssessmentConfidence NormalizeConfidence(RunningAnalysisAssessmentConfidence confidence)
    {
        return Enum.IsDefined(typeof(RunningAnalysisAssessmentConfidence), confidence)
            ? confidence
            : RunningAnalysisAssessmentConfidence.Medium;
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
