namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed record RunningAnalysisCaptureSessionRequest(
    string ExternalEventId,
    string CourseId,
    string CourseName,
    DateTime StartsAt,
    DateTime EndsAt);
