# PR Report: Add Theme Background Artwork

## Summary
- Added generated background artwork for three app themes:
  - `StellarForge`
  - `DarkRomance`
  - `Wildflowers`
- Kept each background theme-specific by applying layout classes only for the selected theme.
- Brightened the existing PaceLetics neon track background slightly so it remains visible behind the UI.

## User Experience
- **Stellar Forge** now uses a subtle rusty white sci-fi hull-panel texture with dark gradients and vignette overlays.
- **Dark Romance** now uses a dim burgundy damask/satin background with abstract intertwined forms.
- **Wildflowers** now uses a soft pastel poppy-and-cornflower background with a cream overlay.
- **PaceLetics** keeps its neon track background, now slightly brighter and less heavily dimmed.
- Content readability is preserved by rendering all images behind dark or light overlay gradients and keeping app bars/drawers translucent with blur.

## Implementation Details
- Updated `MainLayout.razor` to map selected themes to background layout classes:
  - `pl-synthwave-layout`
  - `pl-stellar-forge-layout`
  - `pl-dark-romance-layout`
  - `pl-wildflowers-layout`
- Updated `app-theme.css` to define per-theme background layers:
  - fixed `::before` image layer
  - fixed `::after` overlay gradients and vignettes
  - theme-specific app bar and drawer transparency
- Added generated image assets:
  - `PaceLetics.Web/wwwroot/images/theme/stellar-forge-tech-background.png`
  - `PaceLetics.Web/wwwroot/images/theme/dark-romance-background.png`
  - `PaceLetics.Web/wwwroot/images/theme/wildflowers-background.png`
- Adjusted the existing PaceLetics background treatment:
  - image opacity `0.18` -> `0.22`
  - slightly brighter image filter
  - slightly lighter dark overlays

## Compatibility Notes
- No theme enum values changed.
- No stored theme preferences are invalidated.
- No new runtime dependencies were added.
- Existing Identity page layout remains unchanged; these new theme-specific backgrounds apply to the Blazor app layout.

## Verification
- Ran `dotnet build PaceLeticsFramework.sln --no-restore`.
- Result: build succeeded.
- Existing NuGet advisory warnings for `OpenMcdf` and `SharpCompress` still appear and are unrelated to this PR.
- Verified local static asset delivery:
  - `wildflowers-background.png` returned `200 image/png`.
  - Earlier during implementation, `stellar-forge-tech-background.png` and `dark-romance-background.png` also returned `200 image/png`.
- Confirmed `localhost:5218` is free after verification runs.

## PR
- Branch: `codex/neon-track-theme-background`
- Base: `main`
- Suggested title: `Add generated backgrounds for app themes`
- Suggested description:
  - Adds subtle generated background artwork for Stellar Forge, Dark Romance, and Wildflowers.
  - Slightly brightens the existing PaceLetics neon track background.
  - Uses per-theme layout classes and overlay gradients to keep the UI readable.
