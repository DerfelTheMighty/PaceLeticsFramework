using MudBlazor;

namespace PaceLetics.Web.Services.Theming;

public static class AppThemeCatalog
{
    public static IReadOnlyList<AppThemeDefinition> Themes { get; } =
    [
        new(AppThemeName.PaceLetics, CreatePaceLeticsTheme(), Icons.Material.Filled.DirectionsRun),
        new(AppThemeName.Ocean, CreateOceanTheme(), Icons.Material.Filled.Waves),
        new(AppThemeName.Forest, CreateForestTheme(), Icons.Material.Filled.Park),
        new(AppThemeName.HighContrast, CreateHighContrastTheme(), Icons.Material.Filled.Contrast),
        new(AppThemeName.Wildflowers, CreateWildflowersTheme(), Icons.Material.Filled.LocalFlorist),
        new(AppThemeName.Afterglow, CreateAfterglowTheme(), Icons.Material.Filled.WbTwilight),
        new(AppThemeName.DarkRomance, CreateDarkRomanceTheme(), Icons.Material.Filled.Favorite),
        new(AppThemeName.Maritime, CreateMaritimeTheme(), Icons.Material.Filled.Sailing),
        new(AppThemeName.Tropical, CreateTropicalTheme(), Icons.Material.Filled.BeachAccess)
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
                Primary = "#FF2BD6",
                Secondary = "#00E5FF",
                Tertiary = "#B7FF2A",
                Info = "#00B8FF",
                Success = "#22E87A",
                Warning = "#FFE14A",
                Error = "#FF3B7D",
                Background = "#FFF6FF",
                BackgroundGray = "#F3E8FF",
                Surface = "#FFFFFF",
                AppbarBackground = "#31116E",
                AppbarText = "#FFF7FF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#2B154D",
                TextPrimary = "#241136",
                TextSecondary = "#6C4F82"
            },
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
            light: new PaletteLight
            {
                Primary = "#006C80",
                Secondary = "#6C5CE7",
                Tertiary = "#FFB703",
                Info = "#006C80",
                Success = "#008A74",
                Warning = "#D99500",
                Error = "#D94A3D",
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
                Info = "#35C2DC",
                Success = "#48D6B8",
                Warning = "#FFD166",
                Error = "#FF7F76",
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
                Info = "#3D6FA8",
                Success = "#2E7D32",
                Warning = "#A56B32",
                Error = "#B84A3A",
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
                Info = "#7FB2E8",
                Success = "#78C86B",
                Warning = "#C9975D",
                Error = "#F08A5D",
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
            },
            dark: new PaletteDark
            {
                Primary = "#FFFFFF",
                Secondary = "#00D9FF",
                Tertiary = "#FFD400",
                Info = "#00D9FF",
                Success = "#2EFF8F",
                Warning = "#FFD400",
                Error = "#FF5A5F",
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

    private static MudTheme CreateWildflowersTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#6D8F2E",
                Secondary = "#C55A8A",
                Tertiary = "#E6A23C",
                Info = "#5C7FA8",
                Success = "#6D8F2E",
                Warning = "#C98728",
                Error = "#B95764",
                Background = "#FBFAF3",
                BackgroundGray = "#F0F4E7",
                Surface = "#FFFFFF",
                AppbarBackground = "#4F6F27",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#2F3526",
                TextPrimary = "#2E3327",
                TextSecondary = "#6F755F"
            },
            dark: new PaletteDark
            {
                Primary = "#A8D75F",
                Secondary = "#F08DB8",
                Tertiary = "#F4C36F",
                Info = "#8FBDE8",
                Success = "#A8D75F",
                Warning = "#F4C36F",
                Error = "#F08DB8",
                Background = "#15170F",
                BackgroundGray = "#202519",
                Surface = "#252A1D",
                AppbarBackground = "#0F130A",
                AppbarText = "#F4F8E8",
                DrawerBackground = "#1A2013",
                DrawerText = "#E7EFD7",
                TextPrimary = "#F5FAEA",
                TextSecondary = "#C1CDA9"
            });
    }

    private static MudTheme CreateAfterglowTheme()
    {
        return CreateTheme(
            light: new PaletteLight
            {
                Primary = "#D45D4C",
                Secondary = "#5B5BD6",
                Tertiary = "#F0A202",
                Info = "#5B5BD6",
                Success = "#2F8F6B",
                Warning = "#D98B00",
                Error = "#D45D4C",
                Background = "#FFF8F2",
                BackgroundGray = "#F5ECE7",
                Surface = "#FFFFFF",
                AppbarBackground = "#633D73",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#3B2D35",
                TextPrimary = "#362B31",
                TextSecondary = "#76656D"
            },
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
            light: new PaletteLight
            {
                Primary = "#8F1D3A",
                Secondary = "#3B2C5A",
                Tertiary = "#B7791F",
                Info = "#5B4A8C",
                Success = "#477A53",
                Warning = "#B7791F",
                Error = "#8F1D3A",
                Background = "#FBF5F7",
                BackgroundGray = "#F1E7EC",
                Surface = "#FFFFFF",
                AppbarBackground = "#4A1425",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#35252B",
                TextPrimary = "#302228",
                TextSecondary = "#7B6670"
            },
            dark: new PaletteDark
            {
                Primary = "#E85D84",
                Secondary = "#9A85D6",
                Tertiary = "#E0A23B",
                Info = "#9A85D6",
                Success = "#7DC48F",
                Warning = "#E0A23B",
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
                Primary = "#0B5E78",
                Secondary = "#C43E35",
                Tertiary = "#D9A441",
                Info = "#0B5E78",
                Success = "#2D8C72",
                Warning = "#B8862B",
                Error = "#C43E35",
                Background = "#F5FAFC",
                BackgroundGray = "#E8F1F4",
                Surface = "#FFFFFF",
                AppbarBackground = "#123C52",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#253740",
                TextPrimary = "#20323B",
                TextSecondary = "#637781"
            },
            dark: new PaletteDark
            {
                Primary = "#58C4E0",
                Secondary = "#F47B73",
                Tertiary = "#F0C15D",
                Info = "#58C4E0",
                Success = "#69D2AE",
                Warning = "#F0C15D",
                Error = "#F47B73",
                Background = "#0B141A",
                BackgroundGray = "#121F27",
                Surface = "#172832",
                AppbarBackground = "#061015",
                AppbarText = "#EAF8FC",
                DrawerBackground = "#101C23",
                DrawerText = "#DCECF1",
                TextPrimary = "#EFFAFF",
                TextSecondary = "#A5BBC4"
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
            },
            dark: new PaletteDark
            {
                Primary = "#29D3BD",
                Secondary = "#FF957A",
                Tertiary = "#B7E06A",
                Info = "#5CC8FF",
                Success = "#29D3BD",
                Warning = "#B7E06A",
                Error = "#FF957A",
                Background = "#071815",
                BackgroundGray = "#10241F",
                Surface = "#17302A",
                AppbarBackground = "#04110F",
                AppbarText = "#E9FFFA",
                DrawerBackground = "#0D1D19",
                DrawerText = "#D8F2EC",
                TextPrimary = "#ECFFFB",
                TextSecondary = "#A3C6BE"
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
