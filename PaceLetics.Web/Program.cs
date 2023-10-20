using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;
using CoreLibrary.Models.Athlet;
using MatBlazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaceLetics.Web.Areas.Identity;
using PaceLetics.Web.Data;
using System;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var sqlConnectionString = Environment.GetEnvironmentVariable("PaceLeticsSqlConnString");
//var nonSqlConnectionVar = builder.Configuration.GetSection("EnvVariables").GetSection("PaceLeticsDbConnString").Value;
var nonSqlConnectionString = Environment.GetEnvironmentVariable("PaceLeticsDbConnString");
//var plDbEndPoint = Environment.GetEnvironmentVariable("PaceLeticsDbEndpoint");
//var plDbKey = Environment.GetEnvironmentVariable("PaceLeticsDbKey");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(sqlConnectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<AthleteModelFactory>();
builder.Services.AddTransient<IDataAccess>(x => new DataAccess(nonSqlConnectionString));
builder.Services.AddTransient<IAthleteData, AthleteData>();
builder.Services.AddSingleton<IAthleteService, AthleteService>();
builder.Services.AddMatBlazor();
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

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
