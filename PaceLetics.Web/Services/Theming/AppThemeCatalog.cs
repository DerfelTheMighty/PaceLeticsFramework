using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public static class AppThemeCatalog
{
    public static IReadOnlyList<AppThemeDefinition> Themes { get; } =
    [
        new(AppThemeName.PaceLetics, CreatePaceLeticsTheme(), Icons.Material.Filled.DirectionsRun),
        new(AppThemeName.Ocean, CreateOceanTheme(), Icons.Material.Filled.Waves),
        new(AppThemeName.Forest, CreateForestTheme(), Icons.Material.Filled.Park),
        new(AppThemeName.HighContrast, CreateHighContrastTheme(), Icons.Material.Filled.Contrast)
    ];

    public static MudTheme GetTheme(AppThemeName name)
    {
        return Themes.FirstOrDefault(theme => theme.Name == name)?.Theme
            ?? Themes[0].Theme;
    }

    private static MudTheme CreatePaceLeticsTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#1E7B5F",
                Secondary = "#F2A541",
                Tertiary = "#2F80ED",
                Background = "#F7FAF8",
                BackgroundGray = "#EEF4F1",
                Surface = "#FFFFFF",
                AppbarBackground = "#163D34",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#26322F",
                TextPrimary = "#1F2A27",
                TextSecondary = "#60746E"
            },
            dark: new PaletteDark
            {
                Primary = "#55D6A8",
                Secondary = "#F6BE67",
                Tertiary = "#7AB7FF",
                Background = "#101715",
                BackgroundGray = "#17211E",
                Surface = "#1B2622",
                AppbarBackground = "#0B1210",
                AppbarText = "#EAF8F2",
                DrawerBackground = "#121B18",
                DrawerText = "#DDEBE6",
                TextPrimary = "#EDF8F4",
                TextSecondary = "#A7BBB4"
            });
    }

    private static MudTheme CreateOceanTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#006C80",
                Secondary = "#6C5CE7",
                Tertiary = "#FFB703",
                Background = "#F3FBFD",
                BackgroundGray = "#E6F3F7",
                Surface = "#FFFFFF",
                AppbarBackground = "#004E64",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#183238",
                TextPrimary = "#173037",
                TextSecondary = "#587078"
            },
            dark: new PaletteDark
            {
                Primary = "#35C2DC",
                Secondary = "#A49BFF",
                Tertiary = "#FFD166",
                Background = "#0C171D",
                BackgroundGray = "#13232B",
                Surface = "#182A33",
                AppbarBackground = "#071116",
                AppbarText = "#EAF9FC",
                DrawerBackground = "#101E25",
                DrawerText = "#D7EDF2",
                TextPrimary = "#EDF9FB",
                TextSecondary = "#9DB9C1"
            });
    }

    private static MudTheme CreateForestTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#2E7D32",
                Secondary = "#8E5A2A",
                Tertiary = "#C2410C",
                Background = "#F8FAF3",
                BackgroundGray = "#EFF3E6",
                Surface = "#FFFFFF",
                AppbarBackground = "#24452B",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#283323",
                TextPrimary = "#263122",
                TextSecondary = "#68735F"
            },
            dark: new PaletteDark
            {
                Primary = "#78C86B",
                Secondary = "#C9975D",
                Tertiary = "#F08A5D",
                Background = "#11170E",
                BackgroundGray = "#1A2216",
                Surface = "#202A1C",
                AppbarBackground = "#0C120A",
                AppbarText = "#EEF8EA",
                DrawerBackground = "#151D12",
                DrawerText = "#E0EBD9",
                TextPrimary = "#F0F8EC",
                TextSecondary = "#B3C1AA"
            });
    }

    private static MudTheme CreateHighContrastTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#000000",
                Secondary = "#005FCC",
                Tertiary = "#D00000",
                Background = "#FFFFFF",
                BackgroundGray = "#F0F0F0",
                Surface = "#FFFFFF",
                AppbarBackground = "#000000",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#000000",
                TextPrimary = "#000000",
                TextSecondary = "#222222",
                LinesDefault = "#000000",
                Divider = "#000000"
            },
            dark: new PaletteDark
            {
                Primary = "#FFFFFF",
                Secondary = "#00D9FF",
                Tertiary = "#FFD400",
                Background = "#000000",
                BackgroundGray = "#111111",
                Surface = "#080808",
                AppbarBackground = "#000000",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#000000",
                DrawerText = "#FFFFFF",
                TextPrimary = "#FFFFFF",
                TextSecondary = "#E5E5E5",
                LinesDefault = "#FFFFFF",
                Divider = "#FFFFFF"
            });
    }

    private static MudTheme CreateTheme(PaletteLight light, PaletteDark dark)
    {
        return new MudTheme
        {
            PaletteLight = light,
            PaletteDark = dark,
            LayoutProperties = new LayoutProperties
            {
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "300px"
            },
            Typography = new Typography
            {
                H1 = new H1Typography { FontSize = "2.5rem" },
                H2 = new H2Typography { FontSize = "2rem" },
                H3 = new H3Typography { FontSize = "1.6rem" },
                H4 = new H4Typography { FontSize = "1.3rem" },
                H5 = new H5Typography { FontSize = "1.1rem" },
                H6 = new H6Typography { FontSize = "1rem" },
                Subtitle1 = new Subtitle1Typography { FontSize = "0.95rem" },
                Subtitle2 = new Subtitle2Typography { FontSize = "0.85rem" },
                Body1 = new Body1Typography { FontSize = "0.9rem" },
                Body2 = new Body2Typography { FontSize = "0.8rem" }
            }
        };
    }
}
