namespace PaceLetics.RunningAnalysisModule.Components;

public sealed record RunningAnalysisRecordedVideo(
    Stream Data,
    string ContentType,
    string FileExtension,
    long Size);
