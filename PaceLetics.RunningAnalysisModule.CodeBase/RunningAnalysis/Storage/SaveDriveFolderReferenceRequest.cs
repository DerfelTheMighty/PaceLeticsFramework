namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

public sealed record SaveDriveFolderReferenceRequest(
    string CourseId,
    string ExternalEventId,
    string AthleteUserId,
    string Email,
    string FolderId,
    string FolderUrl);
