using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public sealed record AppThemeDefinition(
    AppThemeName Name,
    string DisplayName,
    MudTheme Theme,
    string Icon,
    bool IsDarkMode,
    PaceZonePalette PaceZones);

public sealed record PaceZonePalette(
    string Free,
    string Recovery,
    string Easy,
    string Marathon,
    string Threshold,
    string Intervall,
    string Repetition);
