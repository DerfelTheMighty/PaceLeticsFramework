namespace PaceLetics.Web.Services.TrainingFeedback;

public interface ITrainingFeedbackRepository
{
    Task<IReadOnlyList<TrainingFeedbackDocument>> GetForAthleteAsync(string athleteUserId);

    Task UpsertAsync(TrainingFeedbackDocument feedback);
}
