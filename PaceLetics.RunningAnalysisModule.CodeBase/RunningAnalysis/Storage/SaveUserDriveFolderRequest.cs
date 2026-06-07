namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

public sealed record SaveUserDriveFolderRequest(
    string AthleteUserId,
    string Email,
    string FolderId,
    string FolderUrl);
