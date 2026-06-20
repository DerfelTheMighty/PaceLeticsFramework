using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.Mates;

public sealed class MateShareRequest
{
    public string CourseId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public sealed class MateAvailabilityDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = MateDocumentTypes.Availability;
    public string AthleteUserId { get; set; } = string.Empty;
    public string AthleteDisplayName { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public int DistanceMeters { get; set; }
    public string PaceKey { get; set; } = string.Empty;
    public int? PaceSecondsPerKilometer { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class MateShareableSession
{
    public string CourseId { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public string PlanId { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public string SessionName { get; init; } = string.Empty;
    public DateTime SessionDate { get; init; }
    public DateTime? StartsAt { get; init; }
    public string Location { get; init; } = string.Empty;
    public int DistanceMeters { get; init; }
    public string PaceKey { get; init; } = string.Empty;
    public int? PaceSecondsPerKilometer { get; init; }
    public bool IsShared { get; init; }
    public string? AvailabilityId { get; init; }
}

public sealed class MateMatch
{
    public MateAvailabilityDocument OwnSession { get; init; } = new();
    public MateAvailabilityDocument MateSession { get; init; } = new();
    public int Score { get; init; }
    public int? PaceDeltaSeconds { get; init; }
    public int DayDelta { get; init; }
    public int DistanceDeltaMeters { get; init; }
}

public sealed class MateOverview
{
    public IReadOnlyList<MateShareableSession> ShareableSessions { get; init; } = Array.Empty<MateShareableSession>();
    public IReadOnlyList<MateAvailabilityDocument> MyAvailabilities { get; init; } = Array.Empty<MateAvailabilityDocument>();
    public IReadOnlyList<MateMatch> Matches { get; init; } = Array.Empty<MateMatch>();
}

public static class MateDocumentTypes
{
    public const string Availability = "mateAvailability";
}
