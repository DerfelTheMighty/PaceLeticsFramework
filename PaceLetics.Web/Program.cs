using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaceLetics.Web.Areas.Identity;
using PaceLetics.Web.Data;
using MudBlazor.Extensions;
using MudBlazor;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Services;
using PaceLetics.TrainingModule.CodeBase.Running.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
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
using PaceLetics.Web.Services.Mates;
using PaceLetics.Web.Services.RunningAnalysis;
using PaceLetics.Web.Services.Theming;
using PaceLetics.Web.Services.Loading;
using PaceLetics.Web.Services.Workouts;
using PaceLetics.TrainingPlanModule.CodeBase.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Services;
using PaceLetics.Web.Localization;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;
using PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive;

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
		options.SignIn.RequireConfirmedAccount = false;
		options.SignIn.RequireConfirmedEmail = false;
		options.Password.RequireNonAlphanumeric = false;
		options.User.RequireUniqueEmail = false;
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
builder.Services.AddSingleton<ITrainingPlanFactory>(x => new TrainingPlanFactory(
    x.GetRequiredService<IRunningSessionFactory>(),
    x.GetRequiredService<IWorkoutCatalog>()));
builder.Services.AddScoped<ITrainingPlanRepository>(_ => new JsonTrainingPlanRepository(trainingPlansPath));
builder.Services.AddScoped<IRunningSessionRepository>(_ => new JsonRunningSessionRepository(legacyRunningSessionsPath));
builder.Services.AddScoped<WorkoutAreaViewModel>();
builder.Services.AddScoped<SelectDifficultyViewModel>();
builder.Services.AddScoped<WorkoutRoomViewModel>();
builder.Services.AddSingleton<IWorkoutCatalogStore, CosmosWorkoutCatalogStore>();
builder.Services.AddSingleton<WorkoutCatalogManagementService>();
builder.Services.AddHostedService<WorkoutCatalogStartupLoader>();
builder.Services.AddSingleton<AthleteModelFactory>();
builder.Services.AddSingleton(builder.Configuration.GetAthleteDataOptions());
builder.Services.AddTransient<IDataAccess>(x => new DataAccess(nonSqlConnectionString));
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
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
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
builder.Services.AddScoped<IMateRepository>(x => x.GetRequiredService<CosmosCourseRepository>());
builder.Services.Configure<GoogleDriveRunningAnalysisOptions>(options =>
{
    builder.Configuration.GetSection(GoogleDriveRunningAnalysisOptions.LegacySectionName).Bind(options);
    builder.Configuration.GetSection(GoogleDriveRunningAnalysisOptions.FlatSectionName).Bind(options);
    builder.Configuration.GetSection(GoogleDriveRunningAnalysisOptions.SectionName).Bind(options);
});
builder.Services.AddScoped<CosmosRunningAnalysisRepository>();
builder.Services.AddScoped<IRunningAnalysisRepository>(x => x.GetRequiredService<CosmosRunningAnalysisRepository>());
builder.Services.AddScoped<IUserDriveFolderRegistry>(x => x.GetRequiredService<CosmosRunningAnalysisRepository>());
builder.Services.AddScoped<IUserDriveFolderRepository>(x => x.GetRequiredService<CosmosRunningAnalysisRepository>());
builder.Services.AddSingleton<IRunningAnalysisClock, SystemRunningAnalysisClock>();
builder.Services.AddScoped<IRunningAnalysisStorageProvider>(x =>
    new GoogleDriveRunningAnalysisStorageProvider(
        x.GetRequiredService<IOptions<GoogleDriveRunningAnalysisOptions>>().Value));
builder.Services.AddScoped<IUserDriveFolderStorageProvider>(x =>
    new GoogleDriveRunningAnalysisStorageProvider(
        x.GetRequiredService<IOptions<GoogleDriveRunningAnalysisOptions>>().Value));
builder.Services.AddScoped<IRunningAnalysisService, RunningAnalysisService>();
builder.Services.AddScoped<IUserDriveFolderService, UserDriveFolderService>();
builder.Services.AddScoped<ICourseRunningAnalysisRegistrationAdapter, CourseRunningAnalysisRegistrationAdapter>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();
builder.Services.AddScoped<IArticleRepository, LocalizedArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IMateService, MateService>();
builder.Services.AddScoped<ThemePreferenceService>();
builder.Services.AddScoped<LoadingStateService>();
builder.Services.AddSingleton<DashboardMessageFeedOptions>();
builder.Services.AddSingleton<TrainingGuardEvaluator>();
builder.Services.AddScoped<IAthleteMessageProvider, ReferenceRunDashboardMessageProvider>();
builder.Services.AddScoped<IAthleteMessageProvider, TrainingGuardDashboardMessageProvider>();
builder.Services.AddScoped<IAthleteMessageProvider, UpcomingTrainingDashboardMessageProvider>();
builder.Services.AddScoped<IAthleteMessageFeedService, AthleteMessageFeedService>();
builder.Services.AddScoped<IAthleteMessageFeedStateStore, CosmosAthleteMessageFeedStateStore>();


//builder.Services.AddMudServices();
//builder.Services.AddMudExtensions();
builder.Services.AddMudServicesWithExtensions(c =>
{
    c.WithDefaultDialogOptions(ex =>
    {
        ex.Position = DialogPosition.BottomRight;
    });
});

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

app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("de")
    .AddSupportedCultures(SupportedCultures.Codes)
    .AddSupportedUICultures(SupportedCultures.Codes));

app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

static async Task SeedIdentityRolesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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

        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Athlete))
            {
                await userManager.AddToRoleAsync(user, ApplicationRoles.Athlete);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Skipping identity role seed because the identity database is not available.");
    }
}
