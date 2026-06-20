namespace PaceLetics.CoreModule.Infrastructure.Models;

public static class PublishedContentTypes
{
    public const string TrainingPlan = "trainingPlan";
    public const string Workout = "workout";
    public const string Exercise = "exercise";
    public const string AcademyLesson = "academyLesson";
    public const string Challenge = "challenge";
}

public sealed class ContentPublication
{
    public string ContentType { get; set; } = string.Empty;

    public string ContentId { get; set; } = string.Empty;

    public FeedTarget Target { get; set; } = FeedTarget.Global();

    public DateTime PublishedAt { get; set; }

    public string PublishedByUserId { get; set; } = string.Empty;

    public DateTime? VisibleFrom { get; set; }

    public DateTime? VisibleUntil { get; set; }

    public bool IsVisibleAt(DateTime utcNow)
    {
        return (VisibleFrom is null || VisibleFrom <= utcNow)
            && (VisibleUntil is null || VisibleUntil > utcNow);
    }

    public bool IsVisibleFor(FeedTarget target, DateTime utcNow)
    {
        return Target.Matches(target) && IsVisibleAt(utcNow);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ContentType))
            throw new InvalidOperationException("A publication content type is required.");

        if (string.IsNullOrWhiteSpace(ContentId))
            throw new InvalidOperationException("A publication content id is required.");

        Target.Validate();

        if (VisibleFrom is not null && VisibleUntil is not null && VisibleUntil <= VisibleFrom)
            throw new InvalidOperationException("VisibleUntil must be later than VisibleFrom.");
    }
}
