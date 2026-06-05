using PaceLetics.Web.Services.Theming;

namespace PaceLetics.Tests;

public sealed class AppThemeCatalogTests
{
    [Fact]
    public void Themes_ContainsDefinitionForEveryThemeName()
    {
        var definedThemeNames = AppThemeCatalog.Themes
            .Select(theme => theme.Name)
            .ToHashSet();

        foreach (var themeName in Enum.GetValues<AppThemeName>())
            Assert.Contains(themeName, definedThemeNames);
    }

    [Fact]
    public void Themes_DoNotContainDuplicateThemeNames()
    {
        var themeNames = AppThemeCatalog.Themes.Select(theme => theme.Name).ToList();

        Assert.Equal(themeNames.Count, themeNames.Distinct().Count());
    }

    [Fact]
    public void Themes_DefinePaceZonePaletteForEveryPaceZone()
    {
        foreach (var theme in AppThemeCatalog.Themes)
        {
            var colors = new[]
            {
                theme.PaceZones.Free,
                theme.PaceZones.Recovery,
                theme.PaceZones.Easy,
                theme.PaceZones.Marathon,
                theme.PaceZones.Threshold,
                theme.PaceZones.Intervall,
                theme.PaceZones.Repetition
            };

            Assert.All(colors, color => Assert.Matches("^#[0-9A-Fa-f]{6}$", color));
            Assert.Equal(colors.Length, colors.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }
    }
}
