using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public static class AppThemeCatalog
{
    public static IReadOnlyList<AppThemeDefinition> Themes { get; } =
    [
        new(AppThemeName.PaceLetics, "PaceLetics", CreatePaceLeticsTheme(), Icons.Material.Filled.DirectionsRun, IsDarkMode: true),
        new(AppThemeName.Ocean, "Ocean", CreateOceanTheme(), Icons.Material.Filled.Waves, IsDarkMode: true),
        new(AppThemeName.Forest, "Forest", CreateForestTheme(), Icons.Material.Filled.Park, IsDarkMode: true),
        new(AppThemeName.HighContrast, "High Contrast", CreateHighContrastTheme(), Icons.Material.Filled.Contrast, IsDarkMode: false),
        new(AppThemeName.Wildflowers, "Wildflowers", CreateWildflowersTheme(), Icons.Material.Filled.LocalFlorist, IsDarkMode: false),
        new(AppThemeName.Afterglow, "Afterglow", CreateAfterglowTheme(), Icons.Material.Filled.WbTwilight, IsDarkMode: true),
        new(AppThemeName.DarkRomance, "Dark Romance", CreateDarkRomanceTheme(), Icons.Material.Filled.Favorite, IsDarkMode: true),
        new(AppThemeName.Maritime, "Maritime", CreateMaritimeTheme(), Icons.Material.Filled.Sailing, IsDarkMode: false),
        new(AppThemeName.Tropical, "Tropical", CreateTropicalTheme(), Icons.Material.Filled.BeachAccess, IsDarkMode: false),
        new(AppThemeName.GoldenHour, "Golden Hour", CreateGoldenHourTheme(), Icons.Material.Filled.WbSunny, IsDarkMode: false),
        new(AppThemeName.StellarForge, "Stellar Forge", CreateStellarForgeTheme(), Icons.Material.Filled.AutoAwesome, IsDarkMode: true),
        new(AppThemeName.SummitBlaze, "Summit Blaze", CreateSummitBlazeTheme(), Icons.Material.Filled.LocalFireDepartment, IsDarkMode: false)
    ];

    public static AppThemeDefinition GetThemeDefinition(AppThemeName name)
    {
        return Themes.FirstOrDefault(theme => theme.Name == name)
            ?? Themes[0];
    }

    public static MudTheme GetTheme(AppThemeName name)
    {
        return GetThemeDefinition(name).Theme;
    }

    private static MudTheme CreatePaceLeticsTheme()
    {
        return CreateTheme(
            dark: new PaletteDark
            {
                Primary = "#FF43D1",
                Secondary = "#00EFFF",
                Tertiary = "#C6FF3D",
                Info = "#38BDF8",
                Success = "#3DFF8F",
                Warning = "#FFE45E",
                Error = "#FF4F8B",
                Background = "#090016",
                BackgroundGray = "#16042E",
                Surface = "#20113B",
                AppbarBackground = "#140029",
                AppbarText = "#FFF3FF",
                DrawerBackground = "#12001F",
                DrawerText = "#F8DDFF",
                TextPrimary = "#FFF5FF",
                TextSecondary = "#D5A9F2"
            });
    }

    private static MudTheme CreateOceanTheme()
    {
        return CreateTheme(
            dark: new PaletteDark
            {
                Primary = "#38C6FF",
                Secondary = "#7AD7F0",
                Tertiary = "#FFD166",
                Info = "#38C6FF",
                Success = "#43D9B8",
                Warning = "#FFD166",
                Error = "#FF7F76",
                Background = "#06172B",
                BackgroundGray = "#0A213B",
                Surface = "#102B49",
                AppbarBackground = "#03101F",
                AppbarText = "#EAF8FF",
                DrawerBackground = "#071B31",
                DrawerText = "#D9F0FA",
                TextPrimary = "#EDF8FF",
                TextSecondary = "#A8C6D9"
            });
    }

    private static MudTheme CreateForestTheme()
    {
        return CreateTheme(
            dark: new PaletteDark
            {
                Primary = "#87D66B",
                Secondary = "#D0A05F",
                Tertiary = "#F08A5D",
                Info = "#88B7E8",
                Success = "#87D66B",
                Warning = "#D0A05F",
                Error = "#F08A5D",
                Background = "#1A1008",
                BackgroundGray = "#24160B",
                Surface = "#2D1E11",
                AppbarBackground = "#120A05",
                AppbarText = "#F5EFE5",
                DrawerBackground = "#211309",
                DrawerText = "#EFE2D0",
                TextPrimary = "#F7EFE5",
                TextSecondary = "#CAB59A"
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
                Info = "#005FCC",
                Success = "#007A3D",
                Warning = "#B77900",
                Error = "#D00000",
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
            });
    }

    private static MudTheme CreateWildflowersTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#D22B2B",
                Secondary = "#4F80E1",
                Tertiary = "#F2C94C",
                Info = "#4F80E1",
                Success = "#5C8A2E",
                Warning = "#D89A1A",
                Error = "#C62828",
                Background = "#FFFDF4",
                BackgroundGray = "#F2F5E8",
                Surface = "#FFFFFF",
                AppbarBackground = "#5C8A2E",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#2E3525",
                TextPrimary = "#283022",
                TextSecondary = "#68745C"
            });
    }

    private static MudTheme CreateAfterglowTheme()
    {
        return CreateTheme(
            dark: new PaletteDark
            {
                Primary = "#FF8A70",
                Secondary = "#A5A0FF",
                Tertiary = "#FFC857",
                Info = "#A5A0FF",
                Success = "#6ED4A8",
                Warning = "#FFC857",
                Error = "#FF8A70",
                Background = "#18121A",
                BackgroundGray = "#241C27",
                Surface = "#2A202E",
                AppbarBackground = "#100B13",
                AppbarText = "#FFF1EC",
                DrawerBackground = "#1D1621",
                DrawerText = "#F0E2EA",
                TextPrimary = "#FFF5F0",
                TextSecondary = "#CDB9C4"
            });
    }

    private static MudTheme CreateDarkRomanceTheme()
    {
        return CreateTheme(
            dark: new PaletteDark
            {
                Primary = "#E85D84",
                Secondary = "#9A85D6",
                Tertiary = "#D6A647",
                Info = "#9A85D6",
                Success = "#7DC48F",
                Warning = "#D6A647",
                Error = "#E85D84",
                Background = "#12090D",
                BackgroundGray = "#211118",
                Surface = "#2A1720",
                AppbarBackground = "#0B0508",
                AppbarText = "#FFEAF0",
                DrawerBackground = "#1A0D13",
                DrawerText = "#F4DDE6",
                TextPrimary = "#FFF0F4",
                TextSecondary = "#C9AAB6"
            });
    }

    private static MudTheme CreateMaritimeTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#2F6F7E",
                Secondary = "#A85F4D",
                Tertiary = "#C9A66B",
                Info = "#3C7F92",
                Success = "#5F8E7A",
                Warning = "#B58A45",
                Error = "#A85F4D",
                Background = "#F7EFE4",
                BackgroundGray = "#E9DDD0",
                Surface = "#FFFDF8",
                AppbarBackground = "#4F7F8C",
                AppbarText = "#FFFDF8",
                DrawerBackground = "#FFFDF8",
                DrawerText = "#3F4B4C",
                TextPrimary = "#343F40",
                TextSecondary = "#71807B"
            });
    }

    private static MudTheme CreateTropicalTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#008C7A",
                Secondary = "#F06A4D",
                Tertiary = "#8BC34A",
                Info = "#007EA7",
                Success = "#008C7A",
                Warning = "#D89C2B",
                Error = "#F06A4D",
                Background = "#F4FCF8",
                BackgroundGray = "#E6F5ED",
                Surface = "#FFFFFF",
                AppbarBackground = "#006B66",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#213733",
                TextPrimary = "#1F3430",
                TextSecondary = "#5D7771"
            });
    }

    private static MudTheme CreateGoldenHourTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#D94F4A",
                Secondary = "#7B5FE8",
                Tertiary = "#F4A62A",
                Info = "#4C7CC7",
                Success = "#2F9A71",
                Warning = "#D98417",
                Error = "#C63F5E",
                Background = "#FFF8EF",
                BackgroundGray = "#F5E8DC",
                Surface = "#FFFFFF",
                AppbarBackground = "#743C6F",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#3E2B35",
                TextPrimary = "#36272F",
                TextSecondary = "#806A73"
            });
    }

    private static MudTheme CreateStellarForgeTheme()
    {
        return CreateTheme(
            dark: new PaletteDark
            {
                Primary = "#FFE81F",
                Secondary = "#4CC9F0",
                Tertiary = "#FFB703",
                Info = "#4CC9F0",
                Success = "#49D4AA",
                Warning = "#FFE81F",
                Error = "#FF5D72",
                Background = "#05070D",
                BackgroundGray = "#0E1322",
                Surface = "#171C2F",
                AppbarBackground = "#02040A",
                AppbarText = "#FFE81F",
                DrawerBackground = "#0A0F1B",
                DrawerText = "#FFEFB0",
                TextPrimary = "#FFF7C2",
                TextSecondary = "#BFC7DA"
            },
            typography: CreateStellarForgeTypography());
    }

    private static MudTheme CreateSummitBlazeTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#FC4C02",
                Secondary = "#1769AA",
                Tertiary = "#212121",
                Info = "#1769AA",
                Success = "#1E8E5A",
                Warning = "#E87500",
                Error = "#D84315",
                Background = "#FFF8F4",
                BackgroundGray = "#F0E7E2",
                Surface = "#FFFFFF",
                AppbarBackground = "#FC4C02",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#242424",
                TextPrimary = "#202020",
                TextSecondary = "#6B5D57"
            });
    }

    private static MudTheme CreateTheme(
        PaletteLight? light = null,
        PaletteDark? dark = null,
        Typography? typography = null)
    {
        return new MudTheme
        {
            PaletteLight = light ?? new PaletteLight(),
            PaletteDark = dark ?? new PaletteDark(),
            LayoutProperties = new LayoutProperties
            {
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "300px"
            },
            Typography = typography ?? CreateDefaultTypography()
        };
    }

    private static Typography CreateDefaultTypography()
    {
        return new Typography
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
        };
    }

    private static Typography CreateStellarForgeTypography()
    {
        var typography = CreateDefaultTypography();
        var displayFamily = new[] { "Arial Black", "Impact", "Segoe UI", "sans-serif" };

        typography.Default = new DefaultTypography { FontFamily = displayFamily };
        typography.H1 = new H1Typography { FontFamily = displayFamily, FontSize = "2.5rem", FontWeight = "700" };
        typography.H2 = new H2Typography { FontFamily = displayFamily, FontSize = "2rem", FontWeight = "700" };
        typography.H3 = new H3Typography { FontFamily = displayFamily, FontSize = "1.6rem", FontWeight = "700" };
        typography.H4 = new H4Typography { FontFamily = displayFamily, FontSize = "1.3rem", FontWeight = "700" };
        typography.H5 = new H5Typography { FontFamily = displayFamily, FontSize = "1.1rem", FontWeight = "700" };
        typography.H6 = new H6Typography { FontFamily = displayFamily, FontSize = "1rem", FontWeight = "700" };

        return typography;
    }
}
