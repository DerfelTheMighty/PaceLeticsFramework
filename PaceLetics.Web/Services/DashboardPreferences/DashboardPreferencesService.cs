namespace PaceLetics.Web.Services.DashboardPreferences;

public static class DashboardSectionKeys
{
    public const string Readiness = "readiness";
    public const string NextSession = "next-session";
    public const string Week = "week";
    public const string Progress = "progress";
    public const string Context = "context";
    public const string Messages = "messages";
    public const string Integrations = "integrations";

    public static IReadOnlyList<string> Defaults { get; } =
    [Readiness, NextSession, Week, Progress, Context, Messages, Integrations];
}

public sealed record DashboardSectionPreference(string Key, bool Visible, int Order);

public sealed class DashboardPreferencesService
{
    private const string StorageKey = "paceletics.dashboard.sections.v1";
    private readonly ClientPreferenceStore _store;

    public DashboardPreferencesService(ClientPreferenceStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<DashboardSectionPreference>> GetAsync()
    {
        var saved = await _store.GetAsync<List<DashboardSectionPreference>>(StorageKey);
        return Normalize(saved);
    }

    public Task SaveAsync(IEnumerable<DashboardSectionPreference> preferences) =>
        _store.SetAsync(StorageKey, Normalize(preferences).ToList());

    public static IReadOnlyList<DashboardSectionPreference> CreateDefault() =>
        DashboardSectionKeys.Defaults
            .Select((key, index) => new DashboardSectionPreference(key, true, index))
            .ToList();

    public static IReadOnlyList<DashboardSectionPreference> Normalize(IEnumerable<DashboardSectionPreference>? saved)
    {
        var byKey = (saved ?? Array.Empty<DashboardSectionPreference>())
            .Where(item => DashboardSectionKeys.Defaults.Contains(item.Key, StringComparer.OrdinalIgnoreCase))
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var result = new List<DashboardSectionPreference>();
        foreach (var item in byKey.Values.OrderBy(item => item.Order))
            result.Add(item with { Order = result.Count });
        foreach (var key in DashboardSectionKeys.Defaults.Where(key => !byKey.ContainsKey(key)))
            result.Add(new DashboardSectionPreference(key, true, result.Count));
        return result;
    }
}
