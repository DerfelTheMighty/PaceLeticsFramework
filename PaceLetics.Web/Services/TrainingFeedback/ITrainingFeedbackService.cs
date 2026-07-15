namespace PaceLetics.Web.Services.TrainingFeedback;

public interface ITrainingFeedbackService
{
    Task<TrainingFeedbackDocument> SaveAsync(
        string athleteUserId,
        TrainingFeedbackInput input,
        string trainingName,
        string source,
        string planId = "",
        string sessionId = "",
        string workoutId = "");

    Task<IReadOnlyList<TrainingFeedbackDocument>> GetRecentAsync(string athleteUserId, int days = 28);

    Task<TrainingRecommendation> GetRecommendationAsync(string athleteUserId, bool hasUpcomingTraining);
}
