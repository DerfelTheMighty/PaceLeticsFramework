namespace PaceLetics.RunningAnalysisModule.Components;

public sealed record RunningAnalysisRecordedVideo(
    byte[] Data,
    string ContentType,
    string FileExtension);
