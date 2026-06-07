# PR Report: Add Dimmed Neon Track Background Theme

## Summary
- Added a subtle retro neon background to the default PaceLetics theme.
- Used an AI-generated neon version of the athletics track as a dimmed full-page background.
- Kept the background intentionally low contrast so it reads as atmosphere behind the UI rather than primary content.
- Renamed the `GoldenHour` theme display name to **Baltic Sunset** while keeping the internal theme key unchanged.

## User Experience
- The PaceLetics theme now has a muted neon sport-track visual identity.
- The generated background shows a pink neon track with blue wireframe hills.
- App content remains readable because the image is rendered at low opacity and covered by dark overlay gradients.
- Identity pages use the same visual treatment as the authenticated Blazor layout for a consistent first impression.
- Other themes remain unchanged.

## Implementation Details
- Added a PaceLetics-specific layout class:
  - `pl-synthwave-layout`
- Applied the class only when the selected app theme is `AppThemeName.PaceLetics`.
- Applied the same background class to Razor Identity pages.
- Added the generated background asset:
  - `PaceLetics.Web/wwwroot/images/theme/paceletics-neon-track-background.png`
- Updated global theme CSS to:
  - render the image as a fixed full-page background
  - set the image layer to `opacity: 0.18`
  - add dark vignette and vertical overlays for readability
  - keep app bar and drawer slightly translucent with blur

## Compatibility Notes
- The internal enum value remains `GoldenHour`, so existing stored theme preferences are not invalidated.
- Only the user-facing display name changes to **Baltic Sunset**.
- No new runtime dependencies were added.

## Verification
- Ran `dotnet test PaceLetics.Tests\PaceLetics.Tests.csproj --no-restore`.
- Result: 111 passed, 0 failed, 0 skipped.
- Verified locally that the generated background image is served as `200 image/png`.
- Verified in browser-computed CSS that the PaceLetics background uses the generated image, is sized with `cover`, and has `opacity: 0.18`.
- Existing NuGet advisory warnings for `OpenMcdf` and `SharpCompress` still appear and are unrelated to this PR.

## PR
- Branch: `codex/neon-track-theme-background`
- Base: `main`
- Suggested title: `Add dimmed neon track background to PaceLetics theme`
