namespace PaceLetics.TrainingModule.CodeBase.Running.Definitions;

public sealed class IntervalSessionDefinition : RunningSessionDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public int? WarmupDistance { get; set; }
    public int? CooldownDistance { get; set; }
    public List<int> Distances { get; set; } = new();
    public List<int>? Recovery { get; set; } = new();
    public List<string> PaceKeys { get; set; } = new();
    public int Sets { get; set; } = 1;
    public int SetRecovery { get; set; } = 0;
}
