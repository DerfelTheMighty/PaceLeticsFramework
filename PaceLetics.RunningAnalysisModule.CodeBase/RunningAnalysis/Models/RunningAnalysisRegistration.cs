namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed record RunningAnalysisRegistration(
    string ExternalEventId,
    string CourseId,
    string EventTitle,
    DateTime StartsAt,
    DateTime EndsAt,
    string AthleteUserId,
    string DisplayName,
    string Email,
    string? RegistrationId = null,
    DateTime? RegisteredAt = null,
    string CourseName = "");
