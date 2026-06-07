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

    public async Task<RunningAnalysisEvent?> GetEventAsync(string analysisEventId, CancellationToken cancellationToken = default)
    {
        var events = await LoadAllAsync<RunningAnalysisEvent>(RunningAnalysisDocumentTypes.Event);
        return events.FirstOrDefault(analysisEvent => analysisEvent.Id == analysisEventId);
    }

    public async Task<RunningAnalysisEvent?> GetEventByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default)
    {
        var events = await LoadAllAsync<RunningAnalysisEvent>(RunningAnalysisDocumentTypes.Event);
        return events.FirstOrDefault(analysisEvent => analysisEvent.ExternalEventId == externalEventId);
    }

    public Task UpsertEventAsync(RunningAnalysisEvent analysisEvent, CancellationToken cancellationToken = default)
    {
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            WithDocumentType(analysisEvent, RunningAnalysisDocumentTypes.Event),
            analysisEvent.CourseId);
    }

    public async Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsAsync(string analysisEventId, CancellationToken cancellationToken = default)
    {
        var participants = await LoadAllAsync<RunningAnalysisParticipant>(RunningAnalysisDocumentTypes.Participant);
        return participants
            .Where(participant => participant.AnalysisEventId == analysisEventId)
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
}
