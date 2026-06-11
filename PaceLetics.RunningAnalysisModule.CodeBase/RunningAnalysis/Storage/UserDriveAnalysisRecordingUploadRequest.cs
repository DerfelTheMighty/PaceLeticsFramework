namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

public sealed record UserDriveAnalysisRecordingUploadRequest(
    string AthleteUserId,
    string AthleteEmail,
    string AnalysisTitle,
    DateTime? AnalysisStartsAt,
    string FileName,
    string ContentType,
    Stream Content);
