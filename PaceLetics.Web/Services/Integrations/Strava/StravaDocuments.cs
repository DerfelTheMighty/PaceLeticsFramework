using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.Integrations.Strava;

public static class StravaDocumentTypes
{
    public const string Connection = "stravaConnection";
    public const string Activity = "stravaActivity";
}

public sealed class StravaConnectionDocument : IVersionedQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string? ETag { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = StravaDocumentTypes.Connection;
    public string AthleteUserId { get; set; } = string.Empty;
    public long StravaAthleteId { get; set; }
    public string StravaAthleteName { get; set; } = string.Empty;
    public string ProtectedAccessToken { get; set; } = string.Empty;
    public string ProtectedRefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public List<string> Scopes { get; set; } = [];
    public DateTime ConnectedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string LastError { get; set; } = string.Empty;
}

public sealed class StravaActivityDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = StravaDocumentTypes.Activity;
    public string AthleteUserId { get; set; } = string.Empty;
    public long StravaActivityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public DateTime StartDateUtc { get; set; }
    public DateTime StartDateLocal { get; set; }
    public double DistanceMeters { get; set; }
    public int MovingTimeSeconds { get; set; }
    public int ElapsedTimeSeconds { get; set; }
    public double TotalElevationGainMeters { get; set; }
    public double AverageSpeedMetersPerSecond { get; set; }
    public double? AverageHeartRate { get; set; }
    public double? MaxHeartRate { get; set; }
    public bool IsCommute { get; set; }
    public bool IsTrainer { get; set; }
    public bool IsManual { get; set; }
    public DateTime ImportedAt { get; set; }

    public string StravaUrl => $"https://www.strava.com/activities/{StravaActivityId}";
}

public static class StravaDocumentIds
{
    public static string Partition(string athleteUserId) => $"integration:strava:{Normalize(athleteUserId)}";
    public static string Connection(string athleteUserId) => $"strava-connection:{Normalize(athleteUserId)}";
    public static string Activity(string athleteUserId, long activityId) => $"strava-activity:{Normalize(athleteUserId)}:{activityId}";

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value)
        ? "empty"
        : value.Trim().Replace(":", "-", StringComparison.Ordinal);
}

public sealed record StravaConnectionStatus(
    bool IsConfigured,
    bool IsConnected,
    string AthleteName = "",
    DateTime? LastSyncAt = null,
    string Error = "",
    IReadOnlyList<string>? Scopes = null);

public sealed record StravaSyncResult(int ImportedActivities, int TotalStoredActivities, DateTime SyncedAt);
