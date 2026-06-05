namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IRunningAnalysisClock
{
    DateTime UtcNow { get; }
}
