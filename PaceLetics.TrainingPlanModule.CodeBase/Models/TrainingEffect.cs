namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed record TrainingEffect(
    string Focus = "",
    string Stimulus = "",
    string Adaptation = "",
    string Recovery = "")
{
    public static TrainingEffect Empty { get; } = new();

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Focus)
        && string.IsNullOrWhiteSpace(Stimulus)
        && string.IsNullOrWhiteSpace(Adaptation)
        && string.IsNullOrWhiteSpace(Recovery);

    public TrainingEffect Normalize()
    {
        return this with
        {
            Focus = Focus?.Trim() ?? string.Empty,
            Stimulus = Stimulus?.Trim() ?? string.Empty,
            Adaptation = Adaptation?.Trim() ?? string.Empty,
            Recovery = Recovery?.Trim() ?? string.Empty
        };
    }
}
