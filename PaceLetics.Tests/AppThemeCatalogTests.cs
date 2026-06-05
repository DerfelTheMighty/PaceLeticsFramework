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
}
