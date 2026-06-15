using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.Web.Services.RunningAnalysis;

public sealed class CosmosRunningAnalysisRepository :
    IRunningAnalysisRepository,
    IUserDriveFolderRegistry,
    IUserDriveFolderRepository
{
    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;

    public CosmosRunningAnalysisRepository(IDataAccess db, AthleteDataOptions options)
    {
        _db = db;
        _options = options;
        _options.Validate();
    }

    public async Task<RunningAnalysisCaptureSession?> GetCaptureSessionAsync(string captureSessionId, CancellationToken cancellationToken = default)
    {
        var captureSessions = await LoadAllAsync<RunningAnalysisCaptureSession>(RunningAnalysisDocumentTypes.CaptureSession);
        var captureSession = captureSessions.FirstOrDefault(captureSession => captureSession.Id == captureSessionId);
        if (captureSession is not null)
            return captureSession;

        var legacyEvent = await GetEventAsync(captureSessionId, cancellationToken);
        return legacyEvent is null ? null : FromLegacyEvent(legacyEvent);
    }

    public async Task<RunningAnalysisCaptureSession?> GetCaptureSessionByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default)
    {
        var captureSessions = await LoadAllAsync<RunningAnalysisCaptureSession>(RunningAnalysisDocumentTypes.CaptureSession);
        var captureSession = captureSessions.FirstOrDefault(captureSession => captureSession.ExternalEventId == externalEventId);
        if (captureSession is not null)
            return captureSession;

        var legacyEvent = await GetEventByExternalEventIdAsync(externalEventId, cancellationToken);
        return legacyEvent is null ? null : FromLegacyEvent(legacyEvent);
    }

    public Task UpsertCaptureSessionAsync(RunningAnalysisCaptureSession captureSession, CancellationToken cancellationToken = default)
    {
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            WithDocumentType(captureSession, RunningAnalysisDocumentTypes.CaptureSession),
            captureSession.CourseId);
    }

    public async Task<RunningAnalysisEvent?> GetEventAsync(string analysisEventId, CancellationToken cancellationToken = default)
    {
        var events = await LoadAllAsync<RunningAnalysisEvent>(RunningAnalysisDocumentTypes.Event);
        var analysisEvent = events.FirstOrDefault(analysisEvent => analysisEvent.Id == analysisEventId);
        if (analysisEvent is not null)
            return analysisEvent;

        var captureSessions = await LoadAllAsync<RunningAnalysisCaptureSession>(RunningAnalysisDocumentTypes.CaptureSession);
        var captureSession = captureSessions.FirstOrDefault(captureSession => captureSession.Id == analysisEventId);
        return captureSession is null ? null : ToLegacyEvent(captureSession);
    }

    public async Task<RunningAnalysisEvent?> GetEventByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default)
    {
        var events = await LoadAllAsync<RunningAnalysisEvent>(RunningAnalysisDocumentTypes.Event);
        var analysisEvent = events.FirstOrDefault(analysisEvent => analysisEvent.ExternalEventId == externalEventId);
        if (analysisEvent is not null)
            return analysisEvent;

        var captureSessions = await LoadAllAsync<RunningAnalysisCaptureSession>(RunningAnalysisDocumentTypes.CaptureSession);
        var captureSession = captureSessions.FirstOrDefault(captureSession => captureSession.ExternalEventId == externalEventId);
        return captureSession is null ? null : ToLegacyEvent(captureSession);
    }

    public Task UpsertEventAsync(RunningAnalysisEvent analysisEvent, CancellationToken cancellationToken = default)
    {
        return UpsertCaptureSessionAsync(FromLegacyEvent(analysisEvent), cancellationToken);
    }

    public async Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsAsync(string analysisEventId, CancellationToken cancellationToken = default)
    {
        var participants = await LoadAllAsync<RunningAnalysisParticipant>(RunningAnalysisDocumentTypes.Participant);
        return participants
            .Where(participant => GetParticipantCaptureSessionId(participant) == analysisEventId)
            .OrderBy(participant => participant.SortOrder)
            .ThenBy(participant => participant.DisplayName)
            .ToList();
    }

    public async Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsForAthleteAsync(string athleteUserId, CancellationToken cancellationToken = default)
    {
        var participants = await LoadAllAsync<RunningAnalysisParticipant>(RunningAnalysisDocumentTypes.Participant);
        return participants
            .Where(participant => participant.AthleteUserId == athleteUserId)
            .ToList();
    }

    public async Task<RunningAnalysisParticipant?> GetParticipantAsync(string analysisEventId, string athleteUserId, CancellationToken cancellationToken = default)
    {
        var participants = await GetParticipantsAsync(analysisEventId, cancellationToken);
        return participants.FirstOrDefault(participant => participant.AthleteUserId == athleteUserId);
    }

    public async Task<RunningAnalysisParticipant?> GetParticipantByIdAsync(string participantId, CancellationToken cancellationToken = default)
    {
        var participants = await LoadAllAsync<RunningAnalysisParticipant>(RunningAnalysisDocumentTypes.Participant);
        return participants.FirstOrDefault(participant => participant.Id == participantId);
    }

    public Task UpsertParticipantAsync(RunningAnalysisParticipant participant, CancellationToken cancellationToken = default)
    {
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            WithDocumentType(participant, RunningAnalysisDocumentTypes.Participant),
            participant.CourseId);
    }

    public async Task<IReadOnlyList<RunningAnalysisRecording>> GetRecordingsForParticipantAsync(string participantId, CancellationToken cancellationToken = default)
    {
        var recordings = await LoadAllAsync<RunningAnalysisRecording>(RunningAnalysisDocumentTypes.Recording);
        return recordings
            .Where(recording => recording.ParticipantId == participantId)
            .OrderBy(recording => recording.AttemptNumber)
            .ToList();
    }

    public Task UpsertRecordingAsync(RunningAnalysisRecording recording, CancellationToken cancellationToken = default)
    {
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            WithDocumentType(recording, RunningAnalysisDocumentTypes.Recording),
            recording.CourseId);
    }

    public async Task<DriveFolderReference?> FindReusableFolderAsync(
        ReusableDriveFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        var references = await LoadAllAsync<RunningAnalysisDriveFolderReferenceDocument>(RunningAnalysisDocumentTypes.FolderReference);
        var reference = references
            .OrderByDescending(reference => reference.UpdatedAt)
            .FirstOrDefault(reference =>
                reference.CourseId == request.CourseId
                && reference.ExternalEventId == request.ExternalEventId
                && reference.AthleteUserId == request.AthleteUserId);

        return reference is null
            ? null
            : new DriveFolderReference(reference.FolderId, reference.FolderUrl);
    }

    public Task SaveFolderReferenceAsync(
        SaveDriveFolderReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var document = new RunningAnalysisDriveFolderReferenceDocument
        {
            Id = FolderReferenceId(request.CourseId, request.ExternalEventId, request.AthleteUserId),
            CourseId = request.CourseId,
            ExternalEventId = request.ExternalEventId,
            AthleteUserId = request.AthleteUserId,
            Email = request.Email,
            FolderId = request.FolderId,
            FolderUrl = request.FolderUrl,
            UpdatedAt = DateTime.UtcNow
        };

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            document,
            document.CourseId);
    }

    public async Task<DriveFolderReference?> FindUserFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return null;

        var reference = await _db.LoadItem<UserDriveFolderReferenceDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            UserFolderReferenceId(athleteUserId.Trim()),
            UserDriveFolderReferenceDocument.PartitionKeyValue);

        return reference is null
            ? null
            : new DriveFolderReference(reference.FolderId, reference.FolderUrl);
    }

    public Task SaveUserFolderAsync(
        SaveUserDriveFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        var document = new UserDriveFolderReferenceDocument
        {
            Id = UserFolderReferenceId(request.AthleteUserId),
            AthleteUserId = request.AthleteUserId,
            Email = request.Email,
            FolderId = request.FolderId,
            FolderUrl = request.FolderUrl,
            UpdatedAt = DateTime.UtcNow
        };

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            document,
            UserDriveFolderReferenceDocument.PartitionKeyValue);
    }

    public Task DeleteUserFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        return _db.DeleteItem<UserDriveFolderReferenceDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            UserFolderReferenceId(athleteUserId),
            UserDriveFolderReferenceDocument.PartitionKeyValue);
    }

    private Task<List<T>> LoadAllAsync<T>(string documentType)
    {
        return _db.LoadData<T>(_options.DatabaseName, _options.CourseContainerName, documentType);
    }

    private static T WithDocumentType<T>(T item, string documentType)
    {
        var property = typeof(T).GetProperty("DocumentType");
        property?.SetValue(item, documentType);
        return item;
    }

    private static string FolderReferenceId(string courseId, string externalEventId, string athleteUserId)
    {
        return $"running-analysis-folder:{courseId}:{externalEventId}:{athleteUserId}";
    }

    private static string UserFolderReferenceId(string athleteUserId)
    {
        return $"user-drive-folder:{athleteUserId.Trim()}";
    }

    private static string GetParticipantCaptureSessionId(RunningAnalysisParticipant participant)
    {
        return string.IsNullOrWhiteSpace(participant.CaptureSessionId)
            ? participant.AnalysisEventId
            : participant.CaptureSessionId;
    }

    private static RunningAnalysisCaptureSession FromLegacyEvent(RunningAnalysisEvent analysisEvent)
    {
        return new RunningAnalysisCaptureSession
        {
            Id = analysisEvent.Id,
            DocumentType = RunningAnalysisDocumentTypes.CaptureSession,
            ExternalEventId = analysisEvent.ExternalEventId,
            CourseId = analysisEvent.CourseId,
            CourseName = analysisEvent.Title,
            Title = analysisEvent.Title,
            StartsAt = analysisEvent.StartsAt,
            EndsAt = analysisEvent.EndsAt,
            Status = analysisEvent.Status,
            DriveFolderId = analysisEvent.DriveFolderId,
            DriveFolderUrl = analysisEvent.DriveFolderUrl,
            CreatedAt = analysisEvent.CreatedAt,
            UpdatedAt = analysisEvent.UpdatedAt
        };
    }

    private static RunningAnalysisEvent ToLegacyEvent(RunningAnalysisCaptureSession captureSession)
    {
        return new RunningAnalysisEvent
        {
            Id = captureSession.Id,
            DocumentType = RunningAnalysisDocumentTypes.Event,
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
}
