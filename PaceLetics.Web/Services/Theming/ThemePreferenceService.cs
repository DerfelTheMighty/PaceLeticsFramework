using Microsoft.JSInterop;
using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public sealed class ThemePreferenceService
{
    private const string ThemeKey = "paceletics.theme.name";
    private const string SchemeKey = "paceletics.theme.scheme";
    private readonly IJSRuntime _js;

    public ThemePreferenceService(IJSRuntime js)
    {
        _js = js;
    }

    public event Action? Changed;

    public AppThemeName ThemeName { get; private set; } = AppThemeName.PaceLetics;
    public AppColorScheme ColorScheme { get; private set; } = AppColorScheme.System;
    public bool SystemPrefersDarkMode { get; private set; }
    public IReadOnlyList<AppThemeDefinition> Themes => AppThemeCatalog.Themes;
    public MudTheme CurrentTheme => AppThemeCatalog.GetTheme(ThemeName);

    public bool IsDarkMode => ColorScheme switch
    {
        AppColorScheme.Dark => true,
        AppColorScheme.Light => false,
        _ => SystemPrefersDarkMode
    };

    public async Task InitializeAsync(bool systemPrefersDarkMode)
    {
        SystemPrefersDarkMode = systemPrefersDarkMode;

        var storedTheme = await GetLocalStorageItemAsync(ThemeKey);
        if (Enum.TryParse(storedTheme, ignoreCase: true, out AppThemeName themeName))
            ThemeName = themeName;

        var storedScheme = await GetLocalStorageItemAsync(SchemeKey);
        if (Enum.TryParse(storedScheme, ignoreCase: true, out AppColorScheme colorScheme))
            ColorScheme = colorScheme;

        NotifyChanged();
    }

    public Task SetSystemPrefersDarkModeAsync(bool systemPrefersDarkMode)
    {
        if (SystemPrefersDarkMode == systemPrefersDarkMode)
            return Task.CompletedTask;

        SystemPrefersDarkMode = systemPrefersDarkMode;

        if (ColorScheme == AppColorScheme.System)
            NotifyChanged();

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

    public async Task SetColorSchemeAsync(AppColorScheme colorScheme)
    {
        if (ColorScheme == colorScheme)
            return;

        ColorScheme = colorScheme;
        await SetLocalStorageItemAsync(SchemeKey, colorScheme.ToString());
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
