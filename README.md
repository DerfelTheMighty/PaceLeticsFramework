# PaceLetics Framework

PaceLetics is a Blazor Server application split into module projects for athlete data, training logic, reusable components, and the web host.

## Project Layout

- `PaceLetics.Web`: Blazor Server host, Identity, DI setup, pages, shared UI, and runtime configuration.
- `AthleteDataAccessLibrary`: Cosmos DB data access and athlete persistence.
- `PaceLetics.CoreModule.Infrastructure`: shared domain primitives, constants, converters, and VDOT/pace models.
- `PaceLetics.AthleteModule.*`: athlete domain services and athlete UI components.
- `PaceLetics.TrainingModule.*`: running session models, workout domain logic, and training UI components.
- `PaceLetics.TrainingPlanModule.CodeBase`: training plan loading and composition across runs and workouts.
- `PaceLetics.Tests`: regression tests for pace lookup, running session resolution, and workout providers.

## Configuration

The web host expects these environment variables:

- `PaceLeticsSqlConnString`: SQL Server connection string for ASP.NET Identity.
- `PaceLeticsDbConnString`: Cosmos DB connection string for athlete data.
- `PaceLeticsSmtpPw`: SMTP password. If unset, `Smtp:Password` from configuration is used.

The remaining runtime settings live in `PaceLetics.Web/appsettings.json`:

- `Smtp`: host, port, user, sender, and optional password fallback.
- `AthleteData`: Cosmos database and athlete container names.

### Strava

Create an API application in the Strava developer settings and configure its authorization callback domain for the PaceLetics host. The OAuth callback route is `/integrations/strava/callback`.

Keep the client secret out of committed configuration. For local development, either copy `PaceLetics.Web/appsettings.Local.example.json` to `appsettings.Local.json` or use user secrets:

```powershell
dotnet user-secrets set "Integrations:Strava:ClientId" "12345" --project PaceLetics.Web
dotnet user-secrets set "Integrations:Strava:ClientSecret" "your-client-secret" --project PaceLetics.Web
dotnet user-secrets set "Integrations:Strava:CallbackUrl" "https://localhost:5001/integrations/strava/callback" --project PaceLetics.Web
```

`CallbackUrl` can be omitted to derive it from the current request host. Set it explicitly behind a reverse proxy unless forwarded headers are configured. The first synchronization imports the previous 90 days by default; later synchronizations are incremental with a one-day overlap. Only running activity summaries are stored, without GPS tracks or route geometry.

## Local Quality Gate

Run the same checks used by the quality workflow:

```powershell
dotnet restore PaceLeticsFramework.sln
dotnet build PaceLeticsFramework.sln --no-restore
dotnet test PaceLeticsFramework.sln --no-build
```

At the moment the build has one known package warning: `NU1902` for transitive `SharpCompress` via `MudBlazor.Extensions`. Dependabot is configured to surface package updates.

## CI

`.github/workflows/quality.yml` runs restore, build, and tests for pushes to `main`/`master` and for pull requests. `.github/dependabot.yml` checks NuGet and GitHub Actions updates weekly.
