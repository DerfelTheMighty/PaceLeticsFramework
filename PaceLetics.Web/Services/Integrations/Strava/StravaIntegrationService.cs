using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace PaceLetics.Web.Services.Integrations.Strava;

public sealed class StravaIntegrationService : IStravaIntegrationService
{
    private const int PageSize = 100;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> SyncGates = new(StringComparer.Ordinal);
    private static readonly HashSet<string> RunningSportTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Run",
        "TrailRun",
        "VirtualRun"
    };

    private readonly IStravaIntegrationRepository _repository;
    private readonly IStravaApiClient _apiClient;
    private readonly StravaTokenProtector _tokenProtector;
    private readonly StravaOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<StravaIntegrationService> _logger;

    public StravaIntegrationService(
        IStravaIntegrationRepository repository,
        IStravaApiClient apiClient,
        StravaTokenProtector tokenProtector,
        IOptions<StravaOptions> options,
        TimeProvider timeProvider,
        ILogger<StravaIntegrationService> logger)
    {
        _repository = repository;
        _apiClient = apiClient;
        _tokenProtector = tokenProtector;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;

    public string CreateAuthorizationUrl(string athleteUserId, string redirectUri, string protectedState)
    {
        EnsureConfigured();
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new ArgumentException("An athlete user id is required.", nameof(athleteUserId));

        return QueryString.Create(
            new Dictionary<string, string?>
            {
                ["client_id"] = _options.ClientId.ToString(CultureInfo.InvariantCulture),
                ["redirect_uri"] = redirectUri,
                ["response_type"] = "code",
                ["approval_prompt"] = "auto",
                ["scope"] = "read,activity:read_all",
                ["state"] = protectedState
            }).ToUriComponent()
            .Insert(0, "https://www.strava.com/oauth/authorize");
    }

    public async Task CompleteAuthorizationAsync(
        string athleteUserId,
        string code,
        string grantedScope,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Strava did not return an authorization code.");

        var token = await _apiClient.ExchangeCodeAsync(code, cancellationToken);
        var scopes = ParseScopes(string.IsNullOrWhiteSpace(token.Scope) ? grantedScope : token.Scope);
        if (!scopes.Contains("activity:read", StringComparer.OrdinalIgnoreCase)
            && !scopes.Contains("activity:read_all", StringComparer.OrdinalIgnoreCase))
        {
            await _apiClient.RevokeAsync(token.RefreshToken, cancellationToken);
            throw new InvalidOperationException("Strava activity access was not granted.");
        }

        if (token.Athlete is null)
        {
            await _apiClient.RevokeAsync(token.RefreshToken, cancellationToken);
            throw new InvalidOperationException("Strava did not return athlete information.");
        }

        var athlete = token.Athlete;
        var existing = await _repository.GetConnectionAsync(athleteUserId, cancellationToken);
        if (existing is not null
            && existing.StravaAthleteId != 0
            && existing.StravaAthleteId != athlete.Id)
        {
            await _repository.DeleteAllAsync(athleteUserId, cancellationToken);
            existing = null;
        }
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var connection = existing ?? new StravaConnectionDocument
        {
            AthleteUserId = athleteUserId,
            ConnectedAt = now
        };
        connection.StravaAthleteId = athlete.Id;
        connection.StravaAthleteName = string.Join(' ', new[] { athlete.FirstName, athlete.LastName }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        connection.ProtectedAccessToken = _tokenProtector.Protect(token.AccessToken);
        connection.ProtectedRefreshToken = _tokenProtector.Protect(token.RefreshToken);
        connection.AccessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(token.ExpiresAt).UtcDateTime;
        connection.Scopes = scopes;
        connection.LastError = string.Empty;
        await _repository.UpsertConnectionAsync(connection, cancellationToken);
    }

    public async Task<StravaConnectionStatus> GetStatusAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new StravaConnectionStatus(false, false);

        var connection = await _repository.GetConnectionAsync(athleteUserId, cancellationToken);
        return connection is null
            ? new StravaConnectionStatus(true, false)
            : new StravaConnectionStatus(
                true,
                true,
                connection.StravaAthleteName,
                connection.LastSyncAt,
                connection.LastError,
                connection.Scopes);
    }

    public Task<IReadOnlyList<StravaActivityDocument>> GetActivitiesAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetActivitiesAsync(athleteUserId, cancellationToken);
    }

    public async Task<StravaSyncResult> SyncAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var gate = SyncGates.GetOrAdd(athleteUserId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var connection = await _repository.GetConnectionAsync(athleteUserId, cancellationToken)
                ?? throw new InvalidOperationException("Strava is not connected.");
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            try
            {
                var accessToken = await GetAccessTokenAsync(connection, now, cancellationToken);
                var afterUtc = connection.LastSyncAt?.AddDays(-1) ?? now.AddDays(-_options.InitialSyncDays);
                var imported = 0;
                var received = 0;

                for (var page = 1; received < _options.MaxActivitiesPerSync; page++)
                {
                    var requested = Math.Min(PageSize, _options.MaxActivitiesPerSync - received);
                    var pageActivities = await _apiClient.GetActivitiesAsync(
                        accessToken,
                        afterUtc,
                        page,
                        requested,
                        cancellationToken);
                    received += pageActivities.Count;

                    var runs = pageActivities
                        .Where(IsRunningActivity)
                        .Select(activity => MapActivity(athleteUserId, activity, now))
                        .ToList();
                    await _repository.UpsertActivitiesAsync(runs, cancellationToken);
                    imported += runs.Count;

                    if (pageActivities.Count < requested)
                        break;
                }

                connection.LastSyncAt = now;
                connection.LastError = string.Empty;
                await _repository.UpsertConnectionAsync(connection, cancellationToken);
                var stored = await _repository.GetActivitiesAsync(athleteUserId, cancellationToken);
                return new StravaSyncResult(imported, stored.Count, now);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                connection.LastError = GetErrorCode(exception);
                await TrySaveErrorAsync(connection);
                throw;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task DisconnectAsync(string athleteUserId, CancellationToken cancellationToken = default)
    {
        var connection = await _repository.GetConnectionAsync(athleteUserId, cancellationToken);
        if (connection is null)
            return;

        var refreshToken = _tokenProtector.Unprotect(connection.ProtectedRefreshToken);
        try
        {
            await _apiClient.RevokeAsync(refreshToken, cancellationToken);
        }
        finally
        {
            await _repository.DeleteAllAsync(athleteUserId, CancellationToken.None);
        }
    }

    private async Task<string> GetAccessTokenAsync(
        StravaConnectionDocument connection,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (connection.AccessTokenExpiresAt > now.AddHours(1))
            return _tokenProtector.Unprotect(connection.ProtectedAccessToken);

        var refreshToken = _tokenProtector.Unprotect(connection.ProtectedRefreshToken);
        var refreshed = await _apiClient.RefreshTokenAsync(refreshToken, cancellationToken);
        connection.ProtectedAccessToken = _tokenProtector.Protect(refreshed.AccessToken);
        connection.ProtectedRefreshToken = _tokenProtector.Protect(refreshed.RefreshToken);
        connection.AccessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(refreshed.ExpiresAt).UtcDateTime;
        await _repository.UpsertConnectionAsync(connection, cancellationToken);
        return refreshed.AccessToken;
    }

    private async Task TrySaveErrorAsync(StravaConnectionDocument connection)
    {
        try
        {
            await _repository.UpsertConnectionAsync(connection, CancellationToken.None);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "The Strava synchronization error for {AthleteUserId} could not be persisted.", connection.AthleteUserId);
        }
    }

    private static bool IsRunningActivity(StravaApiActivity activity)
    {
        return RunningSportTypes.Contains(activity.SportType)
            || string.Equals(activity.Type, "Run", StringComparison.OrdinalIgnoreCase);
    }

    private static StravaActivityDocument MapActivity(string athleteUserId, StravaApiActivity activity, DateTime importedAt)
    {
        return new StravaActivityDocument
        {
            AthleteUserId = athleteUserId,
            StravaActivityId = activity.Id,
            Name = activity.Name,
            SportType = string.IsNullOrWhiteSpace(activity.SportType) ? activity.Type : activity.SportType,
            StartDateUtc = activity.StartDateUtc.ToUniversalTime(),
            StartDateLocal = DateTime.SpecifyKind(activity.StartDateLocal, DateTimeKind.Unspecified),
            DistanceMeters = activity.DistanceMeters,
            MovingTimeSeconds = activity.MovingTimeSeconds,
            ElapsedTimeSeconds = activity.ElapsedTimeSeconds,
            TotalElevationGainMeters = activity.TotalElevationGainMeters,
            AverageSpeedMetersPerSecond = activity.AverageSpeedMetersPerSecond,
            AverageHeartRate = activity.AverageHeartRate,
            MaxHeartRate = activity.MaxHeartRate,
            IsCommute = activity.IsCommute,
            IsTrainer = activity.IsTrainer,
            IsManual = activity.IsManual,
            ImportedAt = importedAt
        };
    }

    private static List<string> ParseScopes(string scopes)
    {
        return scopes.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            StravaApiException { StatusCode: HttpStatusCode.TooManyRequests } => "rate-limit",
            StravaApiException { StatusCode: HttpStatusCode.Unauthorized } => "authorization",
            _ => "sync"
        };
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Strava integration is not configured.");
    }
}
