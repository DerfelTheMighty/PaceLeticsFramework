namespace PaceLetics.Web.Services.TrainingFeedback;

public sealed class TrainingFeedbackService : ITrainingFeedbackService
{
    private readonly ITrainingFeedbackRepository _repository;
    private readonly TimeProvider _timeProvider;

    public TrainingFeedbackService(
        ITrainingFeedbackRepository repository,
        TimeProvider? timeProvider = null)
    {
        _repository = repository;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<TrainingFeedbackDocument> SaveAsync(
        string athleteUserId,
        TrainingFeedbackInput input,
        string trainingName,
        string source,
        string planId = "",
        string sessionId = "",
        string workoutId = "")
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException("Training feedback requires an authenticated athlete.");
        if (input.Effort is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(input), "Effort must be between 1 and 10.");
        if (!Enum.IsDefined(input.Feeling))
            throw new ArgumentOutOfRangeException(nameof(input), "Feeling is invalid.");

        var feedback = new TrainingFeedbackDocument
        {
            AthleteUserId = athleteUserId,
            PlanId = planId,
            SessionId = sessionId,
            WorkoutId = workoutId,
            TrainingName = trainingName,
            Source = source,
            Effort = input.Effort,
            Feeling = input.Feeling,
            Comment = input.Comment,
            CompletedAt = _timeProvider.GetUtcNow().UtcDateTime
        };
        feedback.Normalize();
        await _repository.UpsertAsync(feedback);
        return feedback;
    }

    public async Task<IReadOnlyList<TrainingFeedbackDocument>> GetRecentAsync(string athleteUserId, int days = 28)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<TrainingFeedbackDocument>();

        var cutoff = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-Math.Clamp(days, 1, 365));
        return (await _repository.GetForAthleteAsync(athleteUserId))
            .Where(item => item.CompletedAt >= cutoff)
            .OrderByDescending(item => item.CompletedAt)
            .ToList();
    }

    public async Task<TrainingRecommendation> GetRecommendationAsync(
        string athleteUserId,
        bool hasUpcomingTraining)
    {
        var recent = await GetRecentAsync(athleteUserId, 7);
        if (recent.Count == 0)
        {
            return new TrainingRecommendation(
                hasUpcomingTraining ? TrainingRecommendationKind.Upcoming : TrainingRecommendationKind.NoData,
                0);
        }

        var last = recent[0];
        var lastThreeDays = recent
            .Where(item => item.CompletedAt >= _timeProvider.GetUtcNow().UtcDateTime.AddDays(-3))
            .ToList();
        var hardSessions = lastThreeDays.Count(item => item.Effort >= 8);

        var kind = last.Feeling <= TrainingFeeling.Poor || last.Effort >= 9
            ? TrainingRecommendationKind.PrioritizeRecovery
            : hardSessions >= 2
                ? TrainingRecommendationKind.TakeItEasy
                : TrainingRecommendationKind.OnTrack;

        return new TrainingRecommendation(kind, recent.Count, last.CompletedAt);
    }
}
