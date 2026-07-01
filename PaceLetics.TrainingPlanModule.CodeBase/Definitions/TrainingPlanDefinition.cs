namespace PaceLetics.TrainingPlanModule.CodeBase.Definitions;

public sealed class TrainingPlanDefinition
{
    public int SchemaVersion { get; set; }
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<TrainingPlanBlockDefinition> Blocks { get; set; } = new();
    public List<TrainingSessionDefinition> Sessions { get; set; } = new();
}
