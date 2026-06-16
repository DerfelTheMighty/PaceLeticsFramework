namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisResultRequest
{
    public string CaptureSessionId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public string TrainerUserId { get; set; } = string.Empty;
    public string TrainerDisplayName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool Complete { get; set; }
    public IReadOnlyList<RunningAnalysisAssessmentItem> SideAssessment { get; set; } = Array.Empty<RunningAnalysisAssessmentItem>();
    public IReadOnlyList<RunningAnalysisAssessmentItem> RearAssessment { get; set; } = Array.Empty<RunningAnalysisAssessmentItem>();
}
