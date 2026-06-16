using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentType { get; set; } = string.Empty;
    public string CaptureSessionId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string ExternalEventId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public string AthleteUserId { get; set; } = string.Empty;
    public string AthleteDisplayName { get; set; } = string.Empty;
    public string AnalyzerTrainerUserId { get; set; } = string.Empty;
    public string AnalyzerDisplayName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public RunningAnalysisResultStatus Status { get; set; } = RunningAnalysisResultStatus.Draft;
    public List<RunningAnalysisAssessmentItem> SideAssessment { get; set; } = new();
    public List<RunningAnalysisAssessmentItem> RearAssessment { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public int SideScore { get; set; }
    public int SideMaxScore { get; set; }
    public int RearScore { get; set; }
    public int RearMaxScore { get; set; }
    public int TotalScore { get; set; }
    public int TotalMaxScore { get; set; }
    public string? SideRecordingUrl { get; set; }
    public string? RearRecordingUrl { get; set; }
    public string? ResultDriveFileId { get; set; }
    public string? ResultDriveFileUrl { get; set; }
    public DateTime? AnalyzedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
