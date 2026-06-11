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
    string? DriveFileUrl);
