using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisAssessmentItem
{
    public RunningAnalysisCriterion Criterion { get; set; }
    public RunningAnalysisPerspective Perspective { get; set; } = RunningAnalysisPerspective.Side;
    public RunningAnalysisAssessmentRating Rating { get; set; } = RunningAnalysisAssessmentRating.NotAssessable;
    public RunningAnalysisAssessmentConfidence Confidence { get; set; } = RunningAnalysisAssessmentConfidence.Medium;
    public string Notes { get; set; } = string.Empty;
}
