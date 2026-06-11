namespace PaceLetics.RunningAnalysisModule.Components;

public sealed record RunningAnalysisRecordedVideo(
    string LocalId,
    string AnalysisEventId,
    string ParticipantId,
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
