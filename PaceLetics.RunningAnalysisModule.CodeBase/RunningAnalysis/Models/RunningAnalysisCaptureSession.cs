using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisCaptureSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentType { get; set; } = string.Empty;
    public string ExternalEventId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public RunningAnalysisEventStatus Status { get; set; } = RunningAnalysisEventStatus.Draft;
    public string? DriveFolderId { get; set; }
    public string? DriveFolderUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
