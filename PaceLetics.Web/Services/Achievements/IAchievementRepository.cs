namespace PaceLetics.Web.Services.Achievements;

public interface IAchievementRepository
{
    Task<IReadOnlyList<AchievementDefinitionDocument>> GetDefinitionsAsync();

    Task<AchievementDefinitionDocument?> GetDefinitionAsync(string achievementDefinitionId);

    Task UpsertDefinitionAsync(AchievementDefinitionDocument definition);

    Task DeleteDefinitionAsync(string achievementDefinitionId);

    Task<IReadOnlyList<AthleteAchievementDocument>> GetAwardsForAthleteAsync(string athleteUserId);

    Task<AthleteAchievementDocument?> GetAwardAsync(string athleteUserId, string achievementDefinitionId);

    Task UpsertAwardAsync(AthleteAchievementDocument award);

    Task<IReadOnlyList<AchievementEventDocument>> GetEventsForAthleteAsync(string athleteUserId);

    Task UpsertEventAsync(AchievementEventDocument achievementEvent);

    Task<IReadOnlyList<TrainingSessionCompletionDocument>> GetTrainingSessionCompletionsForAthleteAsync(string athleteUserId);

    Task<TrainingSessionCompletionDocument?> GetTrainingSessionCompletionAsync(
        string athleteUserId,
        string planId,
        string sessionId);

    Task UpsertTrainingSessionCompletionAsync(TrainingSessionCompletionDocument completion);

    Task DeleteTrainingSessionCompletionAsync(string athleteUserId, string planId, string sessionId);
}
