namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed record TrainingSessionAppointment(
    DateTime? StartsAt = null,
    DateTime? EndsAt = null,
    string Location = "",
    string Notes = "",
    string ChangeReason = "")
{
    public static TrainingSessionAppointment Empty { get; } = new();

    public bool IsEmpty =>
        StartsAt is null
        && EndsAt is null
        && string.IsNullOrWhiteSpace(Location)
        && string.IsNullOrWhiteSpace(Notes)
        && string.IsNullOrWhiteSpace(ChangeReason);

    public TrainingSessionAppointment Normalize()
    {
        return this with
        {
            Location = Location?.Trim() ?? string.Empty,
            Notes = Notes?.Trim() ?? string.Empty,
            ChangeReason = ChangeReason?.Trim() ?? string.Empty
        };
    }
}
