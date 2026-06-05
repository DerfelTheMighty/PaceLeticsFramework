namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

public sealed record ReusableDriveFolderRequest(
    string CourseId,
    string ExternalEventId,
    string AthleteUserId,
    string Email);
