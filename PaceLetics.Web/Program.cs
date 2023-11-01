using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;
using CoreLibrary.Models.Athlet;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaceLetics.Web.Areas.Identity;
using PaceLetics.Web.Data;
using MudBlazor.Services;
using MudBlazor.Extensions;
using MudBlazor;
using CoreLibrary.Models.Race;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using VdotModule.Contracts;
using VdotModule;
using VdotModule.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var sqlConnectionString = Environment.GetEnvironmentVariable("PaceLeticsSqlConnString");
//var nonSqlConnectionVar = builder.Configuration.GetSection("EnvVariables").GetSection("PaceLeticsDbConnString").Value;
var nonSqlConnectionString = Environment.GetEnvironmentVariable("PaceLeticsDbConnString");
var mailConnectionString = Environment.GetEnvironmentVariable("PaceLeticsMailConnString");
//var plDbEndPoint = Environment.GetEnvironmentVariable("PaceLeticsDbEndpoint");
//var plDbKey = Environment.GetEnvironmentVariable("PaceLeticsDbKey");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(sqlConnectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(
	options => 
	{
		options.SignIn.RequireConfirmedAccount = false;
		options.SignIn.RequireConfirmedEmail = false;
		options.Password.RequireNonAlphanumeric = false;
		options.User.RequireUniqueEmail = true;
		options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    } ).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddSingleton<AthleteModelFactory>();
builder.Services.AddSingleton<RaceResultModelFactory>();
builder.Services.AddTransient<IDataAccess>(x => new DataAccess(nonSqlConnectionString));
builder.Services.AddTransient<IAthleteData, AthleteData>();
builder.Services.AddSingleton<IAthleteService, AthleteService>();
builder.Services.AddSingleton<IVdotService>(x => (new VdotTableReaderWriter()).FromJson("wwwroot/data/vdot_table.json"));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential 
    // cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.ConsentCookieValue = "true";
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddHttpContextAccessor();


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

app.UseAuthorization();
app.UseCookiePolicy();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseMudExtensions();
app.Run();
