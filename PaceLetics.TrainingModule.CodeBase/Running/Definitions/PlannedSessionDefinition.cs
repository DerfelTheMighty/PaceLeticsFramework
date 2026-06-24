namespace PaceLetics.TrainingModule.CodeBase.Running.Definitions;

public sealed class PlannedSessionDefinition : RunningSessionDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public List<RunningSegmentDefinition> Sequence { get; set; } = new();
}
