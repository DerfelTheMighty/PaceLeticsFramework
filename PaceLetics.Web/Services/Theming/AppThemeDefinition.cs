using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public sealed record AppThemeDefinition(
    AppThemeName Name,
    MudTheme Theme,
    string Icon);
