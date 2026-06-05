# PR Report: Localization Language Expansion

## Summary
- Added Turkish, Danish, Arabic, Russian, French, Chinese, Spanish, and Persian to the supported app cultures.
- Reworked the language selector to render from a shared culture catalog instead of hard-coded German/English entries.
- Added right-to-left document direction support for Arabic and Persian in both the Blazor host and Identity layout.
- Localized visible profile role labels such as athlete and coach, including the profile overview and account management area.
- Added controlled English fallback resource files so newly supported cultures resolve text instead of showing raw resource keys.

## Main Areas Changed
- Supported culture catalog: `PaceLetics.Web/Localization/*`
- Request localization registration: `PaceLetics.Web/Program.cs`
- Language menu: `PaceLetics.Web/Shared/CultureSelector.razor`
- HTML language and direction metadata: `PaceLetics.Web/Pages/_Host.cshtml`, `PaceLetics.Web/Pages/Shared/_Layout.cshtml`
- Profile role localization: `PaceLetics.Web/Pages/Profiles.razor`, `PaceLetics.Web/Resources/Pages/Profiles.*.resx`
- Account profile localization and validation messaging: `PaceLetics.Web/Areas/Identity/Pages/Account/Manage/Index.cshtml*`
- Localization fallback resources: neutral `.resx` files copied from existing English resources.
- New translated resource files for core surfaces: app title fallback, navigation, layout theme labels, profile overview, and account profile management.

## Verification
- `dotnet test PaceLeticsFramework.sln` succeeded.
- Browser smoke check confirmed `culture=ar&ui-culture=ar` renders the Identity layout with `lang="ar"` and `dir="rtl"`.

## Notes
- The new cultures use English fallback resources for screens that do not yet have full native translations. This prevents missing-resource keys from appearing while keeping future translation work incremental.
- Existing package vulnerability warnings for `OpenMcdf` and `SharpCompress` remain unrelated to this change.
- German course seed content still exists in `CourseSeedData.cs`; that is seeded course data rather than profile UI localization.
