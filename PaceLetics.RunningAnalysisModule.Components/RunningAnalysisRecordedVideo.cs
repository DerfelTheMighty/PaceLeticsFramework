namespace PaceLetics.RunningAnalysisModule.Components;

public sealed record RunningAnalysisRecordedVideo(
    string LocalId,
    string AnalysisEventId,
    string AnalysisExternalEventId,
    string CourseId,
    string AnalysisTitle,
    DateTime? AnalysisStartsAt,
    string ParticipantId,
    string AthleteUserId,
    string AthleteEmail,
    string ParticipantName,
    string FileName,
    string ContentType,
    string FileExtension,
    long Size,
    DateTime RecordedAt,
    string StorageLocation,
    string UploadStatus,
    int UploadAttempts,
    DateTime? LastUploadAt,
    string? LastError,
    string? DriveFileUrl,
    string CaptureSessionId = "",
    string CaptureExternalEventId = "",
    string CaptureTitle = "",
    DateTime? CaptureStartsAt = null,
    string Perspective = "side")
{
    public string EffectiveCaptureSessionId => string.IsNullOrWhiteSpace(CaptureSessionId) ? AnalysisEventId : CaptureSessionId;
    public string EffectiveCaptureExternalEventId => string.IsNullOrWhiteSpace(CaptureExternalEventId) ? AnalysisExternalEventId : CaptureExternalEventId;
    public string EffectiveCaptureTitle => string.IsNullOrWhiteSpace(CaptureTitle) ? AnalysisTitle : CaptureTitle;
    public DateTime? EffectiveCaptureStartsAt => CaptureStartsAt ?? AnalysisStartsAt;
}
