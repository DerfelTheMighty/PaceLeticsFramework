namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed record RunningAnalysisEventRequest(
    string ExternalEventId,
    string CourseId,
    string Title,
    DateTime StartsAt,
    DateTime EndsAt);
