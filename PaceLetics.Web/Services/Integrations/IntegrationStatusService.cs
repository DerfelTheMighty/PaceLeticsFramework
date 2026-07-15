namespace PaceLetics.Web.Services.Integrations;

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
    string? Error = null);

public sealed class IntegrationStatusService
{
    private const string StorageKey = "paceletics.integrations.status.v1";
    private readonly ClientPreferenceStore _store;

    public IntegrationStatusService(ClientPreferenceStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<IntegrationStatus>> GetAsync()
    {
        var stored = await _store.GetAsync<List<IntegrationStatus>>(StorageKey) ?? [];
        var byProvider = stored
            .Where(item => IntegrationProviders.All.Contains(item.Provider, StringComparer.OrdinalIgnoreCase))
            .GroupBy(item => item.Provider, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        return IntegrationProviders.All
            .Select(provider => byProvider.TryGetValue(provider, out var status)
                ? status with { Provider = provider }
                : new IntegrationStatus(provider))
            .ToList();
    }

    public async Task RecordAsync(IntegrationStatus status)
    {
        var statuses = (await GetAsync()).ToList();
        var index = statuses.FindIndex(item => string.Equals(item.Provider, status.Provider, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) statuses[index] = status;
        await _store.SetAsync(StorageKey, statuses);
    }
}
