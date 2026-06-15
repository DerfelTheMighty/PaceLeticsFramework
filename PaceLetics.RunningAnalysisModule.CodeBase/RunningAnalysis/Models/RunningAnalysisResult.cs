namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentType { get; set; } = string.Empty;
    public string CaptureSessionId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string AthleteUserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
