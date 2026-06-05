using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;

public sealed class SystemRunningAnalysisClock : IRunningAnalysisClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
