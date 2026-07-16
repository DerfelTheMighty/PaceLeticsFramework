using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaceLetics.Web.Areas.Identity;
using PaceLetics.Web.Data;
using MudBlazor;
using MudBlazor.Services;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Services;
using PaceLetics.TrainingModule.CodeBase.Running.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingModule.CodeBase.Running.Services;
using PaceLetics.AthleteModule.CodeBase.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using PaceLetics.Web.Services;
using PaceLetics.AthleteModule.CodeBase.Services;
using PaceLetics.AthleteModule.CodeBase.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using PaceLetics.Web.Configuration;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.TrainingModule.CodeBase.Workouts.Repositories;
using PaceLetics.Web.ViewModels.Workouts;
using PaceLetics.Web.Services.DashboardMessages;
using PaceLetics.Web.Services.Articles;
using PaceLetics.CoreModule.Infrastructure.Services;
using PaceLetics.Web.Services.Courses;
using PaceLetics.Web.Services.Calendar;
using PaceLetics.Web.Services.Mates;
using PaceLetics.Web.Services.ProfileImages;
using PaceLetics.Web.Services.RunningAnalysis;
using PaceLetics.Web.Services.Theming;
using PaceLetics.Web.Services.Loading;
using PaceLetics.Web.Services.AcademyInfo;
using PaceLetics.Web.Services.Achievements;
using PaceLetics.Web.Services.Localization;
using PaceLetics.Web.Services.Workouts;
using PaceLetics.TrainingPlanModule.CodeBase.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Services;
using PaceLetics.Web.Localization;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;
using PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaceLetics.Web.Services.Health;
using PaceLetics.Web.Services.TrainingPlans;
using PaceLetics.Web.Services.TrainingFeedback;
using PaceLetics.Web.Services.DashboardPreferences;
using PaceLetics.Web.Services.Integrations;
using PaceLetics.Web.Services.Integrations.Strava;
using PaceLetics.Web.Services.Undo;
using PaceLetics.Web.Services.Trainer;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

// Add services to the container.

var sqlConnectionString = PaceLeticsConfiguration.GetRequiredEnvironmentVariable("PaceLeticsSqlConnString");
var nonSqlConnectionString = PaceLeticsConfiguration.GetRequiredEnvironmentVariable("PaceLeticsDbConnString");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(sqlConnectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<ApplicationUser>(
	options => 
	{
		options.SignIn.RequireConfirmedAccount = builder.Configuration.GetValue("IdentitySecurity:RequireConfirmedEmail", false);
		options.SignIn.RequireConfirmedEmail = options.SignIn.RequireConfirmedAccount;
		options.Password.RequireNonAlphanumeric = false;
		options.User.RequireUniqueEmail = true;
		options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    } )
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (modelType, factory) =>
            factory.Create(modelType.DeclaringType ?? modelType);
    });
builder.Services.AddServerSideBlazor();
builder.Services.AddDataProtection()
    .SetApplicationName("PaceLetics");
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream", "image/svg+xml"]);
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var isIdentityRequest = context.Request.Path.StartsWithSegments("/Identity/Account", StringComparison.OrdinalIgnoreCase);
        var isStravaOAuthRequest = context.Request.Path.StartsWithSegments("/integrations/strava", StringComparison.OrdinalIgnoreCase);
        if (!isIdentityRequest && !isStravaOAuthRequest)
            return RateLimitPartition.GetNoLimiter("non-identity");

        var requestKind = isStravaOAuthRequest ? "strava" : "identity";
        var clientKey = $"{requestKind}:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        return RateLimitPartition.GetFixedWindowLimiter(
            clientKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = isStravaOAuthRequest ? 20 : 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
var webRootPath = !string.IsNullOrWhiteSpace(builder.Environment.WebRootPath)
    ? builder.Environment.WebRootPath
    : Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var workoutCatalogPath = Path.Combine(webRootPath, "data", "workouts", "catalog.de.json");
var trainingPlansPath = Path.Combine(webRootPath, "data", "plans");
var legacyRunningSessionsPath = Path.Combine(webRootPath, "data", "intervalls.json");
builder.Services.AddSingleton(new JsonWorkoutCatalogRepository(workoutCatalogPath));
builder.Services.AddSingleton<IWorkoutCatalogRepository>(x => x.GetRequiredService<JsonWorkoutCatalogRepository>());
builder.Services.AddSingleton<IWorkoutCatalogValidator>(x => x.GetRequiredService<JsonWorkoutCatalogRepository>());
builder.Services.AddSingleton<WorkoutCatalogDocument>(x => x.GetRequiredService<IWorkoutCatalogRepository>().Load());
builder.Services.AddSingleton<IExerciseCatalog>(x => new ExerciseCatalog(x.GetRequiredService<WorkoutCatalogDocument>().Exercises));
builder.Services.AddSingleton<IExerciseFactory, ExerciseFactory>();
builder.Services.AddSingleton<IWorkoutCatalog>(x => new WorkoutCatalog(
    x.GetRequiredService<IExerciseCatalog>(),
    x.GetRequiredService<WorkoutCatalogDocument>().Workouts));
builder.Services.AddScoped<IWorkoutFactory, WorkoutFactory>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddSingleton<IRunningSessionFactory, RunningSessionFactory>();
builder.Services.AddSingleton<ITrainingPlanDefinitionValidator>(x =>
    new TrainingPlanDefinitionValidator(x.GetRequiredService<IWorkoutCatalog>()));
builder.Services.AddSingleton<ITrainingPlanFactory>(x => new TrainingPlanFactory(
    x.GetRequiredService<IRunningSessionFactory>(),
    validator: x.GetRequiredService<ITrainingPlanDefinitionValidator>()));
builder.Services.AddSingleton(_ => new JsonTrainingPlanRepository(trainingPlansPath));
builder.Services.AddSingleton<CosmosTrainingPlanRepository>();
builder.Services.AddSingleton<ITrainingPlanRepository>(x => x.GetRequiredService<CosmosTrainingPlanRepository>());
builder.Services.AddHostedService<CosmosTrainingPlanRepository>(x => x.GetRequiredService<CosmosTrainingPlanRepository>());
builder.Services.AddScoped<IRunningSessionRepository>(_ => new JsonRunningSessionRepository(legacyRunningSessionsPath));
builder.Services.AddScoped<WorkoutAreaViewModel>();
builder.Services.AddScoped<SelectDifficultyViewModel>();
builder.Services.AddScoped<WorkoutRoomViewModel>();
builder.Services.AddSingleton<IWorkoutCatalogStore, CosmosWorkoutCatalogStore>();
builder.Services.AddSingleton<WorkoutCatalogManagementService>();
builder.Services.AddHostedService<WorkoutCatalogStartupLoader>();
builder.Services.AddSingleton<AthleteModelFactory>();
builder.Services.AddSingleton(builder.Configuration.GetAthleteDataOptions());
builder.Services.AddSingleton(_ => DataAccess.CreateClient(nonSqlConnectionString));
builder.Services.AddSingleton<IDataAccess, DataAccess>();
builder.Services.AddSingleton(new SqlDatabaseHealthCheck(sqlConnectionString));
builder.Services.AddHealthChecks()
    .AddCheck<SqlDatabaseHealthCheck>("sql", tags: ["ready"])
    .AddCheck<CosmosDatabaseHealthCheck>("cosmos", tags: ["ready"]);
builder.Services.AddTransient<IAthleteData, AthleteData>();
builder.Services.AddScoped<IAthleteService, AthleteService>();
builder.Services.AddSingleton<ICriticalSpeedService, CriticalSpeedService>();
builder.Services.AddSingleton<IVdotService>(x => (new VdotTableReaderWriter()).FromJson("wwwroot/data/vdot_table.json"));
builder.Services.AddSingleton<IPaceModelProvider>(x => (new PaceModelReaderWriter()).ReadPaceModelFromJson("wwwroot/data/pacemodel.json"));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential 
    // cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.ConsentCookieValue = "true";
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .Validate(options =>
    {
        try
        {
            options.Validate();
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }, "SMTP host, port and sender must be configured.")
    .ValidateOnStart();
builder.Services.Configure<TrainerVerificationOptions>(options =>
{
    builder.Configuration.GetSection(TrainerVerificationOptions.SectionName).Bind(options);
    if (string.IsNullOrWhiteSpace(options.Code))
    {
        options.Code = Environment.GetEnvironmentVariable("PaceLeticsTrainerVerificationCode");
    }
});
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<CosmosCourseRepository>();
builder.Services.AddScoped<ICourseRepository>(x => x.GetRequiredService<CosmosCourseRepository>());
builder.Services.AddScoped<IGroupRepository>(x => x.GetRequiredService<CosmosCourseRepository>());
builder.Services.AddScoped<IMateRepository>(x => x.GetRequiredService<CosmosCourseRepository>());
builder.Services.AddHostedService<CourseSeedStartupLoader>();
builder.Services.AddOptions<GoogleDriveRunningAnalysisOptions>()
    .Configure(options =>
    {
        builder.Configuration.GetSection(GoogleDriveRunningAnalysisOptions.LegacySectionName).Bind(options);
        builder.Configuration.GetSection(GoogleDriveRunningAnalysisOptions.FlatSectionName).Bind(options);
        builder.Configuration.GetSection(GoogleDriveRunningAnalysisOptions.SectionName).Bind(options);
    })
    .Validate(options => options.HasValidCredentialShape(),
        "Google Drive OAuth credentials must either all be configured or all be empty.")
    .ValidateOnStart();
builder.Services.AddScoped<CosmosRunningAnalysisRepository>();
builder.Services.AddScoped<IRunningAnalysisRepository>(x => x.GetRequiredService<CosmosRunningAnalysisRepository>());
builder.Services.AddScoped<IUserDriveFolderRegistry>(x => x.GetRequiredService<CosmosRunningAnalysisRepository>());
builder.Services.AddScoped<IUserDriveFolderRepository>(x => x.GetRequiredService<CosmosRunningAnalysisRepository>());
builder.Services.AddSingleton<IRunningAnalysisClock, SystemRunningAnalysisClock>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<GoogleDriveRunningAnalysisStorageProvider>(x =>
    new GoogleDriveRunningAnalysisStorageProvider(
        x.GetRequiredService<IOptions<GoogleDriveRunningAnalysisOptions>>().Value));
builder.Services.AddScoped<IRunningAnalysisStorageProvider>(x => x.GetRequiredService<GoogleDriveRunningAnalysisStorageProvider>());
builder.Services.AddScoped<IUserDriveFolderStorageProvider>(x => x.GetRequiredService<GoogleDriveRunningAnalysisStorageProvider>());
builder.Services.AddScoped<IRunningAnalysisService, RunningAnalysisService>();
builder.Services.AddScoped<IUserDriveFolderService, UserDriveFolderService>();
builder.Services.AddScoped<ICourseRunningAnalysisRegistrationAdapter, CourseRunningAnalysisRegistrationAdapter>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();
builder.Services.AddScoped<ITrainingCalendarService, TrainingCalendarService>();
builder.Services.AddScoped<CosmosAchievementRepository>();
builder.Services.AddScoped<IAchievementRepository>(x => x.GetRequiredService<CosmosAchievementRepository>());
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<CosmosTrainingFeedbackRepository>();
builder.Services.AddScoped<ITrainingFeedbackRepository>(x => x.GetRequiredService<CosmosTrainingFeedbackRepository>());
builder.Services.AddScoped<ITrainingFeedbackService, TrainingFeedbackService>();
builder.Services.AddScoped<AppCultureService>();
builder.Services.AddScoped<IArticleRepository>(x => new MarkdownArticleRepository(
    builder.Environment.ContentRootPath,
    x.GetRequiredService<AppCultureService>()));
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IMateService, MateService>();
builder.Services.AddScoped<IProfileImageService, ProfileImageService>();
builder.Services.AddScoped<IProfileImageStore, CosmosProfileImageStore>();
builder.Services.AddScoped<ThemePreferenceService>();
builder.Services.AddScoped<LoadingStateService>();
builder.Services.AddScoped<ClientPreferenceStore>();
builder.Services.AddScoped<DashboardPreferencesService>();
builder.Services.AddScoped<IntegrationStatusService>();
builder.Services.AddOptions<StravaOptions>()
    .Bind(builder.Configuration.GetSection(StravaOptions.SectionName))
    .Validate(options => options.HasValidCredentialShape(),
        "Strava credentials must either both be configured or both be empty; callback and sync limits must be valid.")
    .ValidateOnStart();
builder.Services.AddHttpClient<IStravaApiClient, StravaApiClient>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PaceLetics/1.0");
});
builder.Services.AddSingleton<StravaTokenProtector>();
builder.Services.AddSingleton<StravaOAuthStateService>();
builder.Services.AddScoped<IStravaIntegrationRepository, CosmosStravaIntegrationRepository>();
builder.Services.AddScoped<IStravaIntegrationService, StravaIntegrationService>();
builder.Services.AddScoped<UndoService>();
builder.Services.AddScoped<TrainerWorkspaceService>();
builder.Services.AddScoped<AcademyInfoService>();
builder.Services.AddSingleton<DashboardMessageFeedOptions>();
builder.Services.AddSingleton<TrainingGuardEvaluator>();
builder.Services.AddScoped<IAthleteMessageProvider, ReferenceRunDashboardMessageProvider>();
builder.Services.AddScoped<IAthleteMessageProvider, TrainingGuardDashboardMessageProvider>();
builder.Services.AddScoped<IAthleteMessageProvider, UpcomingTrainingDashboardMessageProvider>();
builder.Services.AddScoped<IAthleteMessageFeedService, AthleteMessageFeedService>();
builder.Services.AddScoped<IAthleteMessageFeedStateStore, CosmosAthleteMessageFeedStateStore>();


builder.Services.AddMudServices();

var app = builder.Build();

await SeedIdentityRolesAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var extension = Path.GetExtension(context.File.Name);
        if (extension is ".css" or ".js" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" or ".webp" or ".woff" or ".woff2" or ".wav")
            context.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});

app.UseRouting();
app.UseRateLimiter();

var requestLocalizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("de")
    .AddSupportedCultures(SupportedCultures.Codes)
    .AddSupportedUICultures(SupportedCultures.Codes);
requestLocalizationOptions.RequestCultureProviders =
[
    new QueryStringRequestCultureProvider(),
    new CookieRequestCultureProvider()
];
app.UseRequestLocalization(requestLocalizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();
app.MapRazorPages();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapGet("/profile-images/{id}", async (
    string id,
    IProfileImageStore store,
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    var image = await store.GetAsync(id, cancellationToken);
    if (image is null)
        return Results.NotFound();

    context.Response.Headers.CacheControl = "private,max-age=31536000,immutable";
    return Results.File(image.Content, image.ContentType);
}).RequireAuthorization();
app.MapGet("/integrations/strava/connect", (
    HttpContext context,
    IStravaIntegrationService strava,
    StravaOAuthStateService stateService,
    IOptions<StravaOptions> options,
    string? returnUrl) =>
{
    var athleteUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrWhiteSpace(athleteUserId))
        return Results.Unauthorized();
    if (!strava.IsConfigured)
        return Results.Redirect(AppendQuery(StravaOAuthStateService.NormalizeReturnUrl(returnUrl), "strava", "not-configured"));

    var redirectUri = ResolveStravaCallbackUri(context.Request, options.Value);
    var state = stateService.Create(athleteUserId, returnUrl);
    return Results.Redirect(strava.CreateAuthorizationUrl(athleteUserId, redirectUri, state));
}).RequireAuthorization();
app.MapGet("/integrations/strava/callback", async (
    HttpContext context,
    IStravaIntegrationService strava,
    StravaOAuthStateService stateService,
    ILoggerFactory loggerFactory,
    string? code,
    string? scope,
    string? state,
    string? error,
    CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("StravaOAuth");
    var athleteUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrWhiteSpace(athleteUserId))
        return Results.Unauthorized();

    StravaOAuthState oauthState;
    try
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new CryptographicException("OAuth state is missing.");
        oauthState = stateService.Read(state);
        if (!string.Equals(oauthState.AthleteUserId, athleteUserId, StringComparison.Ordinal))
            throw new CryptographicException("OAuth state belongs to a different user.");
    }
    catch (Exception exception) when (exception is CryptographicException or InvalidOperationException)
    {
        logger.LogWarning(exception, "Rejected an invalid Strava OAuth callback state.");
        return Results.Redirect("/Athletes/integrations?strava=invalid-state");
    }

    if (!string.IsNullOrWhiteSpace(error))
        return Results.Redirect(AppendQuery(oauthState.ReturnUrl, "strava", "denied"));
    if (string.IsNullOrWhiteSpace(code))
        return Results.Redirect(AppendQuery(oauthState.ReturnUrl, "strava", "error"));

    try
    {
        await strava.CompleteAuthorizationAsync(athleteUserId, code, scope ?? string.Empty, cancellationToken);
        try
        {
            await strava.SyncAsync(athleteUserId, cancellationToken);
            return Results.Redirect(AppendQuery(oauthState.ReturnUrl, "strava", "connected"));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Strava was connected for {AthleteUserId}, but the initial sync failed.", athleteUserId);
            return Results.Redirect(AppendQuery(oauthState.ReturnUrl, "strava", "connected-sync-failed"));
        }
    }
    catch (InvalidOperationException exception) when (exception.Message.Contains("access", StringComparison.OrdinalIgnoreCase))
    {
        logger.LogInformation(exception, "Strava authorization did not include activity access for {AthleteUserId}.", athleteUserId);
        return Results.Redirect(AppendQuery(oauthState.ReturnUrl, "strava", "scope"));
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        logger.LogError(exception, "Strava authorization failed for {AthleteUserId}.", athleteUserId);
        return Results.Redirect(AppendQuery(oauthState.ReturnUrl, "strava", "error"));
    }
}).RequireAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

static async Task SeedIdentityRolesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityRoleSeed");

    try
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            logger.LogInformation("Skipping identity role seed because database migrations are pending.");
            return;
        }

        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Skipping identity role seed because the identity database is not available.");
    }
}

static string ResolveStravaCallbackUri(HttpRequest request, StravaOptions options)
{
    if (!string.IsNullOrWhiteSpace(options.CallbackUrl))
        return options.CallbackUrl;

    return $"{request.Scheme}://{request.Host}{request.PathBase}/integrations/strava/callback";
}

static string AppendQuery(string localUrl, string name, string value)
{
    var separator = localUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
    return $"{localUrl}{separator}{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
}
