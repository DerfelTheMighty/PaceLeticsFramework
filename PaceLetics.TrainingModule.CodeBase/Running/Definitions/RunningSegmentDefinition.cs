using PaceLetics.TrainingModule.CodeBase.Running.Models;

namespace PaceLetics.TrainingModule.CodeBase.Running.Definitions;

public sealed class RunningSegmentDefinition
{
    public SegmentType Type { get; set; }
    public int Distance { get; set; }
    public string? PaceKey { get; set; }
    public TimeSpan? Duration { get; set; }
}
