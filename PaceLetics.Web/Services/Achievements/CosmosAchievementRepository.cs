using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

namespace PaceLetics.Web.Services.Achievements;

public sealed class CosmosAchievementRepository : IAchievementRepository
{
    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;

    public CosmosAchievementRepository(IDataAccess db, AthleteDataOptions options)
    {
        _db = db;
        _options = options;
        _options.Validate();
    }

    public async Task<IReadOnlyList<AchievementDefinitionDocument>> GetDefinitionsAsync()
    {
        var definitions = await _db.LoadPartitionData<AchievementDefinitionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.DefinitionPartition,
            AchievementDocumentTypes.Definition);

        foreach (var definition in definitions)
            definition.Normalize();

        return definitions
            .OrderBy(definition => definition.Title)
            .ThenBy(definition => definition.Id)
            .ToList();
    }

    public Task<AchievementDefinitionDocument?> GetDefinitionAsync(string achievementDefinitionId)
    {
        return _db.LoadItem<AchievementDefinitionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            achievementDefinitionId,
            AchievementDocumentIds.DefinitionPartition);
    }

    public Task UpsertDefinitionAsync(AchievementDefinitionDocument definition)
    {
        definition.Normalize();
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            definition,
            AchievementDocumentIds.DefinitionPartition);
    }

    public Task DeleteDefinitionAsync(string achievementDefinitionId)
    {
        return _db.DeleteItem<AchievementDefinitionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            achievementDefinitionId,
            AchievementDocumentIds.DefinitionPartition);
    }

    public async Task<IReadOnlyList<AthleteAchievementDocument>> GetAwardsForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<AthleteAchievementDocument>();

        var awards = await _db.LoadPartitionData<AthleteAchievementDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.AthleteAwardPartition(athleteUserId),
            AchievementDocumentTypes.Award);

        foreach (var award in awards)
            award.Normalize();

        return awards
            .OrderByDescending(award => award.AwardedAt)
            .ThenBy(award => award.TitleSnapshot)
            .ToList();
    }

    public Task<AthleteAchievementDocument?> GetAwardAsync(string athleteUserId, string achievementDefinitionId)
    {
        return _db.LoadItem<AthleteAchievementDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.Award(athleteUserId, achievementDefinitionId),
            AchievementDocumentIds.AthleteAwardPartition(athleteUserId));
    }

    public Task UpsertAwardAsync(AthleteAchievementDocument award)
    {
        award.Normalize();
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            award,
            award.CourseId);
    }

    public async Task<IReadOnlyList<AchievementEventDocument>> GetEventsForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<AchievementEventDocument>();

        var events = await _db.LoadPartitionData<AchievementEventDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.AthleteEventPartition(athleteUserId),
            AchievementDocumentTypes.Event);

        foreach (var achievementEvent in events)
            achievementEvent.Normalize();

        return events
            .OrderBy(achievementEvent => achievementEvent.OccurredAt)
            .ToList();
    }

    public Task UpsertEventAsync(AchievementEventDocument achievementEvent)
    {
        achievementEvent.Normalize();
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            achievementEvent,
            achievementEvent.CourseId);
    }

    public async Task<IReadOnlyList<TrainingSessionCompletionDocument>> GetTrainingSessionCompletionsForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<TrainingSessionCompletionDocument>();

        var completions = await _db.LoadPartitionData<TrainingSessionCompletionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.AthleteCompletionPartition(athleteUserId),
            AchievementDocumentTypes.TrainingSessionCompletion);

        foreach (var completion in completions)
            completion.Normalize();

        return completions
            .OrderBy(completion => completion.CompletedAt)
            .ToList();
    }

    public Task<TrainingSessionCompletionDocument?> GetTrainingSessionCompletionAsync(
        string athleteUserId,
        string planId,
        string sessionId)
    {
        return _db.LoadItem<TrainingSessionCompletionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.TrainingSessionCompletion(athleteUserId, planId, sessionId),
            AchievementDocumentIds.AthleteCompletionPartition(athleteUserId));
    }

    public Task UpsertTrainingSessionCompletionAsync(TrainingSessionCompletionDocument completion)
    {
        completion.Normalize();
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            completion,
            completion.CourseId);
    }

    public Task DeleteTrainingSessionCompletionAsync(string athleteUserId, string planId, string sessionId)
    {
        return _db.DeleteItem<TrainingSessionCompletionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AchievementDocumentIds.TrainingSessionCompletion(athleteUserId, planId, sessionId),
            AchievementDocumentIds.AthleteCompletionPartition(athleteUserId));
    }
}
