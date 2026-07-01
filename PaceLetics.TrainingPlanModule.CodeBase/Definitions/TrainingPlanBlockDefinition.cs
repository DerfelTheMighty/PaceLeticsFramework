namespace PaceLetics.TrainingPlanModule.CodeBase.Definitions;

public sealed class TrainingPlanBlockDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Focus { get; set; } = "";
    public string Structure { get; set; } = "";
    public string Description { get; set; } = "";
    public int Order { get; set; }
    public List<string> SessionIds { get; set; } = new();
}
