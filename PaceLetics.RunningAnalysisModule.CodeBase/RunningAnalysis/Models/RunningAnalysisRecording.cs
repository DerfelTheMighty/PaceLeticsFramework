using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisRecording
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentType { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CaptureSessionId { get; set; } = string.Empty;
    public string AnalysisEventId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrimary { get; set; }
    public RunningAnalysisUploadStatus UploadStatus { get; set; } = RunningAnalysisUploadStatus.Captured;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "video/webm";
    public string? DriveFileId { get; set; }
    public string? DriveFileUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
