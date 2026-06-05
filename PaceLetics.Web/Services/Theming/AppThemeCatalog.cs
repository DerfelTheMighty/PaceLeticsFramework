using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public static class AppThemeCatalog
{
    public static IReadOnlyList<AppThemeDefinition> Themes { get; } =
    [
        new(AppThemeName.PaceLetics, "PaceLetics", CreatePaceLeticsTheme(), Icons.Material.Filled.DirectionsRun, IsDarkMode: true, PaceZones: CreatePaceLeticsPaceZones()),
        new(AppThemeName.Ocean, "Ocean", CreateOceanTheme(), Icons.Material.Filled.Waves, IsDarkMode: true, PaceZones: CreateOceanPaceZones()),
        new(AppThemeName.Forest, "Forest", CreateForestTheme(), Icons.Material.Filled.Park, IsDarkMode: true, PaceZones: CreateForestPaceZones()),
        new(AppThemeName.HighContrast, "High Contrast", CreateHighContrastTheme(), Icons.Material.Filled.Contrast, IsDarkMode: false, PaceZones: CreateHighContrastPaceZones()),
        new(AppThemeName.Wildflowers, "Wildflowers", CreateWildflowersTheme(), Icons.Material.Filled.LocalFlorist, IsDarkMode: false, PaceZones: CreateWildflowersPaceZones()),
        new(AppThemeName.Afterglow, "Afterglow", CreateAfterglowTheme(), Icons.Material.Filled.WbTwilight, IsDarkMode: true, PaceZones: CreateAfterglowPaceZones()),
        new(AppThemeName.DarkRomance, "Dark Romance", CreateDarkRomanceTheme(), Icons.Material.Filled.Favorite, IsDarkMode: true, PaceZones: CreateDarkRomancePaceZones()),
        new(AppThemeName.Maritime, "Maritime", CreateMaritimeTheme(), Icons.Material.Filled.Sailing, IsDarkMode: false, PaceZones: CreateMaritimePaceZones()),
        new(AppThemeName.Tropical, "Tropical", CreateTropicalTheme(), Icons.Material.Filled.BeachAccess, IsDarkMode: false, PaceZones: CreateTropicalPaceZones()),
        new(AppThemeName.GoldenHour, "Golden Hour", CreateGoldenHourTheme(), Icons.Material.Filled.WbSunny, IsDarkMode: false, PaceZones: CreateGoldenHourPaceZones()),
        new(AppThemeName.StellarForge, "Stellar Forge", CreateStellarForgeTheme(), Icons.Material.Filled.AutoAwesome, IsDarkMode: true, PaceZones: CreateStellarForgePaceZones()),
        new(AppThemeName.SummitBlaze, "Summit Blaze", CreateSummitBlazeTheme(), Icons.Material.Filled.LocalFireDepartment, IsDarkMode: false, PaceZones: CreateSummitBlazePaceZones())
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

    private static PaceZonePalette CreatePaceLeticsPaceZones()
    {
        return new(
            Free: "#8A7EA6",
            Recovery: "#00EFFF",
            Easy: "#3DFF8F",
            Marathon: "#C6FF3D",
            Threshold: "#FFE45E",
            Intervall: "#FF8A3D",
            Repetition: "#FF43D1");
    }

    private static PaceZonePalette CreateOceanPaceZones()
    {
        return new(
            Free: "#8FA9BD",
            Recovery: "#7AD7F0",
            Easy: "#43D9B8",
            Marathon: "#38C6FF",
            Threshold: "#FFD166",
            Intervall: "#FF8F70",
            Repetition: "#FF5E8A");
    }

    private static PaceZonePalette CreateForestPaceZones()
    {
        return new(
            Free: "#BBAA92",
            Recovery: "#9FD0FF",
            Easy: "#9FE68E",
            Marathon: "#F1C97D",
            Threshold: "#F4DD72",
            Intervall: "#F59F79",
            Repetition: "#FF8494");
    }

    private static PaceZonePalette CreateHighContrastPaceZones()
    {
        return new(
            Free: "#666666",
            Recovery: "#005FCC",
            Easy: "#007A3D",
            Marathon: "#5A35B1",
            Threshold: "#B77900",
            Intervall: "#D00000",
            Repetition: "#000000");
    }

    private static PaceZonePalette CreateWildflowersPaceZones()
    {
        return new(
            Free: "#8C9277",
            Recovery: "#77A8FF",
            Easy: "#73B66E",
            Marathon: "#63C6AD",
            Threshold: "#F2B84B",
            Intervall: "#FF9078",
            Repetition: "#F26D9B");
    }

    private static PaceZonePalette CreateAfterglowPaceZones()
    {
        return new(
            Free: "#A899B0",
            Recovery: "#A5A0FF",
            Easy: "#6ED4A8",
            Marathon: "#FFC857",
            Threshold: "#FF9F5A",
            Intervall: "#FF6B70",
            Repetition: "#E56AD6");
    }

    private static PaceZonePalette CreateDarkRomancePaceZones()
    {
        return new(
            Free: "#B98FA2",
            Recovery: "#C6A9FF",
            Easy: "#93DEA4",
            Marathon: "#F2C56D",
            Threshold: "#FF9C8A",
            Intervall: "#FF8CAB",
            Repetition: "#F27AD5");
    }

    private static PaceZonePalette CreateMaritimePaceZones()
    {
        return new(
            Free: "#7E918E",
            Recovery: "#7ED5E6",
            Easy: "#77C9A5",
            Marathon: "#5CBFD2",
            Threshold: "#F3CC7C",
            Intervall: "#FF9B85",
            Repetition: "#D58BC1");
    }

    private static PaceZonePalette CreateTropicalPaceZones()
    {
        return new(
            Free: "#6F8C84",
            Recovery: "#46BFE3",
            Easy: "#1BC7AA",
            Marathon: "#B6E65E",
            Threshold: "#F5BF4A",
            Intervall: "#FF9078",
            Repetition: "#F06FA9");
    }

    private static PaceZonePalette CreateGoldenHourPaceZones()
    {
        return new(
            Free: "#8E7B71",
            Recovery: "#4C7CC7",
            Easy: "#2F9A71",
            Marathon: "#7B5FE8",
            Threshold: "#F4A62A",
            Intervall: "#D94F4A",
            Repetition: "#C63F8D");
    }

    private static PaceZonePalette CreateStellarForgePaceZones()
    {
        return new(
            Free: "#8B92A8",
            Recovery: "#4CC9F0",
            Easy: "#49D4AA",
            Marathon: "#FFE81F",
            Threshold: "#FFB703",
            Intervall: "#FF5D72",
            Repetition: "#C77DFF");
    }

    private static PaceZonePalette CreateSummitBlazePaceZones()
    {
        return new(
            Free: "#756D68",
            Recovery: "#1769AA",
            Easy: "#1E8E5A",
            Marathon: "#7B55C7",
            Threshold: "#E87500",
            Intervall: "#FC4C02",
            Repetition: "#D84373");
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
                Primary = "#9FE68E",
                Secondary = "#F1C97D",
                Tertiary = "#F59F79",
                Info = "#9FD0FF",
                Success = "#9FE68E",
                Warning = "#F1C97D",
                Error = "#FF8494",
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
                Primary = "#F26D85",
                Secondary = "#77A8FF",
                Tertiary = "#FFE06E",
                Info = "#77A8FF",
                Success = "#73B66E",
                Warning = "#F2B84B",
                Error = "#E94F66",
                Background = "#FFF7D9",
                BackgroundGray = "#F5EDBF",
                Surface = "#FFFFFF",
                AppbarBackground = "#73B66E",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#2E3525",
                TextPrimary = "#283022",
                TextSecondary = "#68725C"
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
                Primary = "#FF8CAB",
                Secondary = "#C6A9FF",
                Tertiary = "#F2C56D",
                Info = "#C6A9FF",
                Success = "#93DEA4",
                Warning = "#F2C56D",
                Error = "#FF8CAB",
                Background = "#201018",
                BackgroundGray = "#2A1520",
                Surface = "#321B27",
                AppbarBackground = "#5C2237",
                AppbarText = "#FFEAF0",
                DrawerBackground = "#26131D",
                DrawerText = "#F8DDE8",
                TextPrimary = "#FFF2F6",
                TextSecondary = "#D9B6C4"
            });
    }

    private static MudTheme CreateMaritimeTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#5CBFD2",
                Secondary = "#FF9B85",
                Tertiary = "#F3CC7C",
                Info = "#7ED5E6",
                Success = "#77C9A5",
                Warning = "#E8B85F",
                Error = "#E87866",
                Background = "#EAF7F8",
                BackgroundGray = "#D4ECEE",
                Surface = "#FFFFFF",
                AppbarBackground = "#3E9EAF",
                AppbarText = "#FFFDF8",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#3F4B4C",
                TextPrimary = "#343F40",
                TextSecondary = "#637C7A"
            });
    }

    private static MudTheme CreateTropicalTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#1BC7AA",
                Secondary = "#FF9078",
                Tertiary = "#B6E65E",
                Info = "#46BFE3",
                Success = "#1BC7AA",
                Warning = "#F5BF4A",
                Error = "#F06A69",
                Background = "#E6FAEF",
                BackgroundGray = "#CCF1DF",
                Surface = "#FFFFFF",
                AppbarBackground = "#15A997",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#213733",
                TextPrimary = "#1F3430",
                TextSecondary = "#587A72"
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
