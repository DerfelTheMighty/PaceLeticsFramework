namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

public sealed record UserDriveFolderRequest(
    string AthleteUserId,
    string Email,
    string DisplayName);
