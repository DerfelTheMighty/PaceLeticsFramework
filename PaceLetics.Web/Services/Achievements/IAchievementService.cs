namespace PaceLetics.Web.Services.Achievements;

public interface IAchievementService
{
    Task<IReadOnlyList<AchievementDefinitionDocument>> GetDefinitionsAsync(bool includeUnpublished = false);

    Task<AchievementDefinitionDocument?> GetDefinitionAsync(string achievementDefinitionId);

    Task<AchievementDefinitionDocument> SaveDefinitionAsync(
        AchievementDefinitionRequest request,
        string requestingUserId);

    Task DeleteDefinitionAsync(string achievementDefinitionId, string requestingUserId);

    Task<IReadOnlyList<AthleteAchievementDocument>> GetAwardsForAthleteAsync(string athleteUserId);

    Task<IReadOnlyList<TrainingSessionCompletionDocument>> GetTrainingSessionCompletionsForAthleteAsync(string athleteUserId);

    Task<AchievementEvaluationResult> SetTrainingSessionCompletionAsync(
        string athleteUserId,
        string planId,
        string sessionId,
        bool isCompleted);

    Task<AchievementEvaluationResult> RecordWorkoutCompletedAsync(
        string athleteUserId,
        string workoutId,
        string workoutName);

    Task<AchievementEvaluationResult> EvaluateAthleteAsync(string athleteUserId);
}
