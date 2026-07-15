using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.TrainingFeedback;

public static class TrainingFeedbackDocumentTypes
{
    public const string Feedback = "trainingFeedback";
}

public enum TrainingFeeling
{
    VeryPoor = 1,
    Poor = 2,
    Neutral = 3,
    Good = 4,
    VeryGood = 5
}

public enum TrainingRecommendationKind
{
    NoData,
    Upcoming,
    OnTrack,
    TakeItEasy,
    PrioritizeRecovery
}

public sealed record TrainingFeedbackInput(
    int Effort,
    TrainingFeeling Feeling,
    string Comment = "");

public sealed record TrainingRecommendation(
    TrainingRecommendationKind Kind,
    int FeedbackCount,
    DateTime? LastFeedbackAt = null);

public sealed class TrainingFeedbackDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = TrainingFeedbackDocumentTypes.Feedback;
    public string AthleteUserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string WorkoutId { get; set; } = string.Empty;
    public string TrainingName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Effort { get; set; }
    public TrainingFeeling Feeling { get; set; } = TrainingFeeling.Neutral;
    public string Comment { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }

    public void Normalize()
    {
        AthleteUserId = AthleteUserId?.Trim() ?? string.Empty;
        PlanId = PlanId?.Trim() ?? string.Empty;
        SessionId = SessionId?.Trim() ?? string.Empty;
        WorkoutId = WorkoutId?.Trim() ?? string.Empty;
        TrainingName = TrainingName?.Trim() ?? string.Empty;
        Source = string.IsNullOrWhiteSpace(Source) ? "training" : Source.Trim();
        Comment = Comment?.Trim() ?? string.Empty;
        Effort = Math.Clamp(Effort, 1, 10);
        if (!Enum.IsDefined(Feeling))
            Feeling = TrainingFeeling.Neutral;

        if (CompletedAt == default)
            CompletedAt = DateTime.UtcNow;

        CourseId = TrainingFeedbackDocumentIds.Partition(AthleteUserId);
        DocumentType = TrainingFeedbackDocumentTypes.Feedback;
        if (string.IsNullOrWhiteSpace(Id))
            Id = TrainingFeedbackDocumentIds.Create(this);
    }
}

public static class TrainingFeedbackDocumentIds
{
    public static string Partition(string athleteUserId) =>
        $"training-feedback:{Normalize(athleteUserId)}";

    public static string Create(TrainingFeedbackDocument feedback)
    {
        if (!string.IsNullOrWhiteSpace(feedback.PlanId)
            && !string.IsNullOrWhiteSpace(feedback.SessionId))
        {
            return $"training-feedback:session:{Normalize(feedback.AthleteUserId)}:{Normalize(feedback.PlanId)}:{Normalize(feedback.SessionId)}";
        }

        return $"training-feedback:workout:{Normalize(feedback.AthleteUserId)}:{feedback.CompletedAt:yyyyMMddHHmmssfff}:{Guid.NewGuid():N}";
    }

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value)
        ? "empty"
        : value.Trim().Replace(":", "-", StringComparison.Ordinal);
}
