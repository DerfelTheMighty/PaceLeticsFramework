namespace PaceLetics.Web.Services.Calendar;

public static class TrainingCalendarItemKinds
{
    public const string TrainingSession = "trainingSession";
    public const string CourseDate = "courseDate";
    public const string CourseEvent = "courseEvent";
}

public sealed class TrainingCalendarItem
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public bool HasTime { get; init; }
    public string Location { get; init; } = string.Empty;
    public string CourseId { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public string PlanId { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public bool IsRegistered { get; init; }
}
