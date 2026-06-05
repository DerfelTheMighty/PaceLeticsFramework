# PR Report: Missing Localization

## Summary
- Localized newly introduced course and profile UI surfaces with German and English `.resx` resources.
- Replaced hard-coded user-facing course service exceptions with localizable resource lookups and safe German fallbacks.
- Added localization for remaining workout, athlete dialog, dashboard, Identity account, and small legacy view strings.

## Main Areas Changed
- Athlete course overview: `MyCourses.razor`
- Trainer course management: `CourseManagement.razor`
- Public profiles: `Profiles.razor`
- Dashboard next training tile and legacy shared views
- Identity login/register/manage PageModels and views
- Workout and athlete module residual button/fallback strings

## Verification
- `dotnet build PaceLeticsFramework.sln` succeeded.
- `dotnet test PaceLeticsFramework.sln --no-build` succeeded: 56 passed, 0 failed.

## Notes
- Build still reports existing package vulnerability warnings for `OpenMcdf` and `SharpCompress`; these are unrelated to this localization change.
- Technical configuration exceptions remain in English where they are intended for operators/developers rather than end users.
