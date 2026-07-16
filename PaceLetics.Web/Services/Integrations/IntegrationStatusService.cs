namespace PaceLetics.Web.Services.Integrations;

using PaceLetics.Web.Services.Integrations.Strava;

public static class IntegrationProviders
{
    public const string Garmin = "garmin";
    public const string Apple = "apple";
    public const string Coros = "coros";
    public const string Strava = "strava";

    public static IReadOnlyList<string> All { get; } = [Garmin, Apple, Coros, Strava];
}

public sealed record IntegrationStatus(
    string Provider,
    bool IsConnected = false,
    DateTime? LastSyncAt = null,
    bool HasReadySession = false,
    string? Error = null,
    bool IsAvailable = false);

public sealed class IntegrationStatusService
{
    private const string StorageKey = "paceletics.integrations.status.v1";
    private readonly ClientPreferenceStore _store;
    private readonly IStravaIntegrationService _strava;
    private readonly ILogger<IntegrationStatusService> _logger;

    public IntegrationStatusService(
        ClientPreferenceStore store,
        IStravaIntegrationService strava,
        ILogger<IntegrationStatusService> logger)
    {
        _store = store;
        _strava = strava;
        _logger = logger;
    }

    public async Task<IReadOnlyList<IntegrationStatus>> GetAsync(string? athleteUserId = null)
    {
        var stored = await _store.GetAsync<List<IntegrationStatus>>(StorageKey) ?? [];
        var byProvider = stored
            .Where(item => IntegrationProviders.All.Contains(item.Provider, StringComparer.OrdinalIgnoreCase))
            .GroupBy(item => item.Provider, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var statuses = IntegrationProviders.All
            .Select(provider => byProvider.TryGetValue(provider, out var status)
                ? status with { Provider = provider }
                : new IntegrationStatus(provider))
            .ToList();

        if (!string.IsNullOrWhiteSpace(athleteUserId))
        {
            var index = statuses.FindIndex(status => status.Provider == IntegrationProviders.Strava);
            try
            {
                var strava = await _strava.GetStatusAsync(athleteUserId);
                statuses[index] = new IntegrationStatus(
                    IntegrationProviders.Strava,
                    strava.IsConnected,
                    strava.LastSyncAt,
                    strava.IsConnected,
                    string.IsNullOrWhiteSpace(strava.Error) ? null : strava.Error,
                    strava.IsConfigured);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "The Strava integration status could not be loaded for {AthleteUserId}.", athleteUserId);
                statuses[index] = new IntegrationStatus(
                    IntegrationProviders.Strava,
                    Error: "status",
                    IsAvailable: _strava.IsConfigured);
            }
        }

        return statuses;
    }

    public async Task RecordAsync(IntegrationStatus status)
    {
        var statuses = (await GetAsync()).ToList();
        var index = statuses.FindIndex(item => string.Equals(item.Provider, status.Provider, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) statuses[index] = status;
        await _store.SetAsync(StorageKey, statuses);
    }
}
