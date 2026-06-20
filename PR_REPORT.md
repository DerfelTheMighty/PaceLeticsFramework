# PR Report: Compress Theme Background Assets

## Summary
- Converted all theme background images from PNG to JPG because the assets do not use transparency.
- Updated the theme CSS to reference the new JPG files.
- Kept each image at its original pixel dimensions so background positioning and cover behavior remain unchanged.
- Reduced total theme background payload from approximately 16.48 MB to 1.44 MB.

## Asset Size Impact
| Theme background | Previous size | New size | Reduction |
| --- | ---: | ---: | ---: |
| `dark-romance-background` | 2,401.4 KB | 200.5 KB | 91.7% |
| `forest-prune-background` | 1,742.5 KB | 91.6 KB | 94.7% |
| `maritime-dune-background` | 2,766.6 KB | 246.6 KB | 91.1% |
| `ocean-watercolor-background` | 2,576.2 KB | 229.8 KB | 91.1% |
| `paceletics-neon-track-background` | 2,067.3 KB | 186.7 KB | 91.0% |
| `stellar-forge-tech-background` | 3,024.0 KB | 354.7 KB | 88.3% |
| `wildflowers-background` | 2,295.0 KB | 162.3 KB | 92.9% |

## Implementation Details
- Replaced the PNG theme assets under `PaceLetics.Web/wwwroot/images/theme/` with JPG equivalents:
  - `dark-romance-background.jpg`
  - `forest-prune-background.jpg`
  - `maritime-dune-background.jpg`
  - `ocean-watercolor-background.jpg`
  - `paceletics-neon-track-background.jpg`
  - `stellar-forge-tech-background.jpg`
  - `wildflowers-background.jpg`
- Updated `PaceLetics.Web/wwwroot/css/app-theme.css` to use the new `.jpg` paths.
- Removed the old `.png` files from the theme asset directory.

## Compatibility Notes
- No CSS layout, opacity, filter, or positioning values changed.
- No theme names, enum values, or persisted preferences changed.
- No runtime dependencies were added.
- JPG conversion is safe for these assets because all original files were 24-bit RGB images without alpha transparency.

## Verification
- Confirmed all new JPG files decode successfully and preserve the original dimensions.
- Confirmed there are no remaining source references to the old theme PNG paths.
- Ran `dotnet build .\PaceLeticsFramework.sln`.
- Result: build succeeded.
- Existing warnings remain unrelated to this change:
  - NuGet advisory warnings for `OpenMcdf` and `SharpCompress`.
  - Existing Blazor/code warnings in athlete and training components.

## PR
- Branch: `codex/ocean-forest-theme-backgrounds`
- Base: `main`
- Suggested title: `Compress theme background assets`
- Suggested description:
  - Converts theme background images from PNG to JPG where transparency is not needed.
  - Updates theme CSS references to the optimized JPG assets.
  - Reduces total theme background payload from about 16.48 MB to 1.44 MB.
  - Keeps original image dimensions and existing theme styling intact.
