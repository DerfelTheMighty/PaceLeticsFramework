using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaceLetics.Web.Areas.Identity;
using PaceLetics.Web.Data;
using MudBlazor.Extensions;
using MudBlazor;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Services;
using PaceLetics.AthleteModule.CodeBase.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using PaceLetics.Web.Services;
using PaceLetics.AthleteModule.CodeBase.Services;
using PaceLetics.AthleteModule.CodeBase.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Localization;
using PaceLetics.Web.Configuration;
using PaceLetics.WorkoutModule.CodeBase.Models;
using PaceLetics.WorkoutModule.CodeBase.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var sqlConnectionString = PaceLeticsConfiguration.GetRequiredEnvironmentVariable("PaceLeticsSqlConnString");
var nonSqlConnectionString = PaceLeticsConfiguration.GetRequiredEnvironmentVariable("PaceLeticsDbConnString");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(sqlConnectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(
	options => 
	{
		options.SignIn.RequireConfirmedAccount = false;
		options.SignIn.RequireConfirmedEmail = false;
		options.Password.RequireNonAlphanumeric = false;
		options.User.RequireUniqueEmail = false;
		options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    } ).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
var webRootPath = !string.IsNullOrWhiteSpace(builder.Environment.WebRootPath)
    ? builder.Environment.WebRootPath
    : Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var workoutCatalogPath = Path.Combine(webRootPath, "data", "workouts", "catalog.de.json");
builder.Services.AddSingleton<IWorkoutCatalogRepository>(new JsonWorkoutCatalogRepository(workoutCatalogPath));
builder.Services.AddSingleton<WorkoutCatalogDocument>(x => x.GetRequiredService<IWorkoutCatalogRepository>().Load());
builder.Services.AddSingleton<IExerciseProvider>(x => new ExerciseProvider(x.GetRequiredService<WorkoutCatalogDocument>().Exercises));
builder.Services.AddSingleton<IWorkoutCatalog>(x => new WorkoutCatalog(
    x.GetRequiredService<IExerciseProvider>(),
    x.GetRequiredService<WorkoutCatalogDocument>().Workouts));
builder.Services.AddScoped<IWorkoutFactory, WorkoutFactory>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddSingleton<AthleteModelFactory>();
builder.Services.AddSingleton(builder.Configuration.GetAthleteDataOptions());
builder.Services.AddTransient<IDataAccess>(x => new DataAccess(nonSqlConnectionString));
builder.Services.AddTransient<IAthleteData, AthleteData>();
builder.Services.AddScoped<IAthleteService, AthleteService>();
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
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();


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

var supportedCultures = new[] { "de", "en" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("de")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseAuthorization();
app.UseCookiePolicy();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
