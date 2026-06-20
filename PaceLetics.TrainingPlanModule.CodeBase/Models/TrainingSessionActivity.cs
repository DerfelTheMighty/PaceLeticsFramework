namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed record TrainingSessionActivity(
    string Title = "",
    string Description = "",
    int DurationSeconds = 0,
    string ReferenceId = "",
    string ActivityType = "")
{
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Title)
        && string.IsNullOrWhiteSpace(Description)
        && DurationSeconds == 0
        && string.IsNullOrWhiteSpace(ReferenceId)
        && string.IsNullOrWhiteSpace(ActivityType);

    public TrainingSessionActivity Normalize()
    {
        return this with
        {
            Title = Title?.Trim() ?? string.Empty,
            Description = Description?.Trim() ?? string.Empty,
            ReferenceId = ReferenceId?.Trim() ?? string.Empty,
            ActivityType = ActivityType?.Trim() ?? string.Empty
        };
    }
}
