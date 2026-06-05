# PR Report: Theme Catalog Refresh

## Summary
- Reworked theme selection so each theme now controls whether it renders in light or dark mode.
- Removed the separate Light/Dark/System color-scheme selector from the app bar theme menu.
- Moved visible theme names into the theme catalog as English display names, making English the single source for theme labels.
- Removed the `ElderVale` theme from the enum, catalog, menu, and legacy layout resources.
- Simplified the theme catalog by keeping only the active palette for each theme instead of maintaining unused light/dark variants.

## Theme Direction
- Dark themes: PaceLetics, Ocean, Forest, Afterglow, Dark Romance, Stellar Forge.
- Light themes: High Contrast, Wildflowers, Maritime, Tropical, Golden Hour, Summit Blaze.
- Ocean now uses a deeper dark-blue background.
- Forest now uses a dark-brown background.
- Wildflowers has a brighter poppy/cornflower-inspired palette.
- Maritime is light with a warmer beach/shabby-chic coastal palette.
- Stellar Forge is dark with stronger Star-Wars-like yellow and heavier display typography.

## Main Areas Changed
- Theme catalog and palettes: `PaceLetics.Web/Services/Theming/AppThemeCatalog.cs`
- Theme metadata: `PaceLetics.Web/Services/Theming/AppThemeDefinition.cs`
- Theme enum cleanup: `PaceLetics.Web/Services/Theming/AppThemeName.cs`
- Theme preference behavior: `PaceLetics.Web/Services/Theming/ThemePreferenceService.cs`
- App bar theme menu: `PaceLetics.Web/Shared/MainLayout.razor`
- Removed color-scheme enum: `PaceLetics.Web/Services/Theming/AppColorScheme.cs`
- Removed obsolete translated theme and scheme labels from `PaceLetics.Web/Resources/Shared/MainLayout*.resx`

## Verification
- `dotnet test PaceLetics.Tests\PaceLetics.Tests.csproj --filter AppThemeCatalogTests` succeeded.
- Search confirmed no remaining `AppColorScheme`, `ColorScheme`, `Scheme_*`, `ElderVale`, or localized `Theme_*` references in `PaceLetics.Web` and `PaceLetics.Tests`.

## Notes
- Existing NuGet vulnerability warnings for `OpenMcdf` and `SharpCompress` still appear during test restore/build and are unrelated to this change.
- Existing analyzer/nullability warnings in component projects remain unrelated to this change.
