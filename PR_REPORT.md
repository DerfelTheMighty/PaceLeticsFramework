# PR Report: Theme-Aware Pace and Race Color Codes

## Summary
- Replaced pace-zone PNG icons with theme-aware color-coded markers.
- Added a dedicated seven-step pace-zone palette for every app theme.
- Extended the pace zones page to display `Free`, `Recovery`, `E`, `M`, `T`, `I`, and `R`.
- Reused the same colored pace markers in the training plan detail view.
- Replaced race distance PNG icons with compact color-coded race labels.
- Removed obsolete pace and race PNG assets from the web project.

## Implementation Details
- Added `PaceZonePalette` to theme definitions so every `AppThemeDefinition` carries colors for:
  - `Free`
  - `Recovery`
  - `Easy`
  - `Marathon`
  - `Threshold`
  - `Intervall`
  - `Repetition`
- Exposed the active theme's pace-zone palette as CSS variables from `MainLayout`.
- Updated shared app CSS with reusable zone classes:
  - `.pl-pace-zone-free`
  - `.pl-pace-zone-recovery`
  - `.pl-pace-zone-easy`
  - `.pl-pace-zone-marathon`
  - `.pl-pace-zone-threshold`
  - `.pl-pace-zone-intervall`
  - `.pl-pace-zone-repetition`
- Reworked `PaceInfo` so it renders text-based color markers instead of image icons.
- Added `Free` and `Recovery` entries to the pace zones page, including localized English and German text.
- Added colored pace markers to the training plan bottom-sheet segment details by mapping segment `PaceKey` values to the same CSS zone classes.
- Reworked `RaceCard` so race distances render as colored labels such as `1K`, `3K`, `5K`, `10K`, `15K`, `21K`, and `42K`.
- Removed now-unused PNG references from `PaceLetics.Web.csproj`.

## Removed Assets
- `epace.png`
- `mpace.png`
- `tpace.png`
- `ipace.png`
- `rpace.png`
- `icon_1k.png`
- `icon_3k.png`
- `icon_5k.png`
- `icon_10k.png`
- `icon_15k.png`
- `icon_21k.png`

## Verification
- `dotnet test PaceLetics.Tests\PaceLetics.Tests.csproj --no-restore` passed earlier with 104 tests.
- A later normal `dotnet test` attempt was blocked because a Visual Studio-launched `PaceLetics.Web` process held files in `bin\Debug`.
- To avoid stopping the developer's Visual Studio server, verification was rerun with a separate output directory:
  - `dotnet build PaceLetics.Web\PaceLetics.Web.csproj --no-restore -p:OutDir=%TEMP%\paceletics-verify-build\`
  - Result: build succeeded.
- Browser verification of the pace zones page confirmed:
  - Seven pace cards render.
  - Pace cards contain no images.
  - Pace-zone colors are applied through the active theme CSS variables.
- Repository search confirmed no remaining references to removed pace or race icon PNG filenames.

## Notes
- Existing NuGet advisory warnings for `OpenMcdf` and `SharpCompress` still appear during build/test and are unrelated to this PR.
- Existing analyzer/nullability warnings remain unrelated to this change.
- The PR intentionally keeps the new markers text-based and theme-driven instead of introducing replacement image assets.
