using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public sealed record AppThemeDefinition(
    AppThemeName Name,
    string DisplayName,
    MudTheme Theme,
    string Icon,
    bool IsDarkMode);
