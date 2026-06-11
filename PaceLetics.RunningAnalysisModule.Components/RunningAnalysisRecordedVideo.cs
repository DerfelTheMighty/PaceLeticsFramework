namespace PaceLetics.RunningAnalysisModule.Components;

public sealed record RunningAnalysisRecordedVideo(
    string LocalId,
    string AnalysisEventId,
    string ParticipantId,
    string FileName,
    string ContentType,
    string FileExtension,
    long Size,
    DateTime RecordedAt);
