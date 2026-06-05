using Microsoft.JSInterop;
using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public sealed class ThemePreferenceService
{
    private const string ThemeKey = "paceletics.theme.name";
    private readonly IJSRuntime _js;

    public ThemePreferenceService(IJSRuntime js)
    {
        _js = js;
    }

    public event Action? Changed;

    public AppThemeName ThemeName { get; private set; } = AppThemeName.PaceLetics;
    public IReadOnlyList<AppThemeDefinition> Themes => AppThemeCatalog.Themes;
    public AppThemeDefinition CurrentThemeDefinition => AppThemeCatalog.GetThemeDefinition(ThemeName);
    public MudTheme CurrentTheme => AppThemeCatalog.GetTheme(ThemeName);
    public bool IsDarkMode => CurrentThemeDefinition.IsDarkMode;

    public async Task InitializeAsync(bool systemPrefersDarkMode)
    {
        var storedTheme = await GetLocalStorageItemAsync(ThemeKey);
        if (Enum.TryParse(storedTheme, ignoreCase: true, out AppThemeName themeName))
            ThemeName = themeName;

        NotifyChanged();
    }

    public Task SetSystemPrefersDarkModeAsync(bool systemPrefersDarkMode)
    {
        return Task.CompletedTask;
    }

    public async Task SetThemeAsync(AppThemeName themeName)
    {
        if (ThemeName == themeName)
            return;

        ThemeName = themeName;
        await SetLocalStorageItemAsync(ThemeKey, themeName.ToString());
        NotifyChanged();
    }

    private async Task<string?> GetLocalStorageItemAsync(string key)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }

    private async Task SetLocalStorageItemAsync(string key, string value)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
