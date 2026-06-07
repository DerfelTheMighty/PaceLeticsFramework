# PR Report: Add Ocean and Forest Theme Backgrounds

## Summary
- Added generated background artwork for the `Ocean` and `Forest` app themes.
- Ocean uses a dark watercolor wash with layered currents and subtle teal/gold linework.
- Forest uses a centered ivory-white abstract tree form with sparse thin branches and very small leaves, tuned for mobile center crops.
- Kept backgrounds theme-specific by applying layout classes only for the selected theme.

## User Experience
- **Ocean** now has a calm dark-blue watercolor background that supports the existing navy/teal palette.
- **Forest** now has a warm dark-brown background with a central ivory-white, highly abstract tree form, so the motif remains visible on smartphone viewports.
- Both themes use fixed background image layers plus gradient/vignette overlays so cards, navigation, and text remain readable.
- App bars and drawers are slightly translucent with blur to keep the theme artwork present without competing with the UI.

## Implementation Details
- Updated `MainLayout.razor` to map additional selected themes to layout classes:
  - `AppThemeName.Ocean` -> `pl-ocean-layout`
  - `AppThemeName.Forest` -> `pl-forest-layout`
- Updated `app-theme.css` with:
  - `pl-ocean-layout`
  - `pl-forest-layout`
  - fixed `::before` image layers
  - fixed `::after` gradient and vignette overlays
  - theme-specific app bar and drawer transparency
- Added generated image assets:
  - `PaceLetics.Web/wwwroot/images/theme/ocean-watercolor-background.png`
  - `PaceLetics.Web/wwwroot/images/theme/forest-prune-background.png`

## Compatibility Notes
- No theme enum values changed.
- No stored theme preferences are invalidated.
- No new runtime dependencies were added.
- Existing Identity page layout remains unchanged; these theme-specific backgrounds apply to the Blazor app layout.

## Verification
- Ran `dotnet build PaceLeticsFramework.sln --no-restore`.
- Result: build succeeded.
- Existing NuGet advisory warnings for `OpenMcdf` and `SharpCompress` still appear and are unrelated to this PR.
- Confirmed the generated assets are valid PNG files with expected landscape dimensions.

## PR
- Branch: `codex/ocean-forest-theme-backgrounds`
- Base: `main`
- Suggested title: `Add Ocean and Forest theme backgrounds`
- Suggested description:
  - Adds watercolor-inspired Ocean background artwork.
  - Adds a centered abstract white Forest tree background for better mobile visibility.
  - Uses per-theme layout classes and overlays to keep the UI readable.
