using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PaceLetics.AthleteModule.Components;
using PaceLetics.RunningAnalysisModule.Components;
using PaceLetics.TrainingModule.Components.Running;
using PaceLetics.TrainingModule.Components.Workouts;
using PaceLetics.Web.Areas.Identity.Pages.Account;
using PaceLetics.Web.Areas.Identity.Pages.Account.Manage;
using PaceLetics.Web.Localization;
using PaceLetics.Web.Pages;
using PaceLetics.Web.Pages.Athletes;
using PaceLetics.Web.Pages.Trainers;
using PaceLetics.Web.Services.Courses;
using PaceLetics.Web.Shared;

namespace PaceLetics.Tests;

public sealed class ComponentLocalizationTests
{
    [Theory]
    [InlineData("de", "DifficultyDialog_Start", "Starten")]
    [InlineData("en", "DifficultyDialog_Start", "Start")]
    [InlineData("tr", "DifficultyDialog_Start", "Başlat")]
    [InlineData("ar", "DifficultyDialog_Start", "ابدأ")]
    public void WorkoutResources_ResolveKnownKeysWithAppResourcePath(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<WorkoutResources>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("de", "TrainingCard_Title", "Training")]
    [InlineData("en", "TrainingCard_Title", "Training")]
    [InlineData("fa", "TrainingCard_Title", "تمرین")]
    public void RunningResources_ResolveKnownKeysWithAppResourcePath(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<RunningResources>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("tr", "WelcomeText", "Merhaba {0}, işte güncel antrenman durumun.")]
    [InlineData("es", "Title", "Panel")]
    public void DashboardResources_ResolveTranslatedKeysForNewCultures(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<Dashboard>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("es", "VdotCard_Title", "Tu VDOT")]
    [InlineData("fr", "RaceCard_Edit", "Modifier la course")]
    public void AthleteResources_ResolveTranslatedKeysForNewCultures(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<AthleteResources>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("de", "InterventionGuidance_Title", "Interventionsphilosophie")]
    [InlineData("en", "InterventionGuidance_Title", "Intervention philosophy")]
    public void RunningAnalysisResources_ResolveInterventionGuidanceKeys(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<RunningAnalysisResources>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Fact]
    public void TrainerPageResources_ResolveTranslatedKeysForNewCultures()
    {
        var value = GetLocalizedString<CourseManagement>("fr", "Title");

        Assert.False(value.ResourceNotFound);
        Assert.Equal("Gérer les cours", value.Value);
    }

    [Fact]
    public void CourseServiceResources_ResolveTranslatedKeysForNewCultures()
    {
        var value = GetLocalizedString<CourseService>("zh", "CourseNotFound");

        Assert.False(value.ResourceNotFound);
        Assert.Equal("未找到课程。", value.Value);
    }

    [Fact]
    public void IdentityModelResources_ResolveTranslatedKeysForNewCultures()
    {
        var invalidLogin = GetLocalizedString<LoginModel>("es", "InvalidLoginAttempt");
        var rememberMe = GetLocalizedString<LoginModel>("es", "RememberMe");
        var passwordMatch = GetLocalizedString<RegisterModel>("fr", "ValidationPasswordMatch");
        var newPasswordMatch = GetLocalizedString<ChangePasswordModel>("es", "ValidationNewPasswordMatch");

        Assert.False(invalidLogin.ResourceNotFound);
        Assert.Equal("Intento de inicio de sesión no válido.", invalidLogin.Value);

        Assert.False(rememberMe.ResourceNotFound);
        Assert.Equal("¿Recordarme?", rememberMe.Value);

        Assert.False(passwordMatch.ResourceNotFound);
        Assert.Equal("Le mot de passe et sa confirmation ne correspondent pas.", passwordMatch.Value);

        Assert.False(newPasswordMatch.ResourceNotFound);
        Assert.Equal("La nueva contraseña y la confirmación no coinciden.", newPasswordMatch.Value);
    }

    [Theory]
    [InlineData("tr", "RoleAthlete", "Sporcu")]
    [InlineData("da", "RoleTrainer", "Træner")]
    [InlineData("ar", "RoleAthlete", "رياضي")]
    [InlineData("ru", "RoleTrainer", "Тренер")]
    [InlineData("fr", "RoleAthlete", "Athlète")]
    [InlineData("zh", "RoleTrainer", "教练")]
    [InlineData("es", "RoleAthlete", "Atleta")]
    [InlineData("fa", "RoleTrainer", "مربی")]
    public void ProfileResources_ResolveRoleLabelsForNewCultures(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<Profiles>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("tr", "Nav_Profiles", "Profiller")]
    [InlineData("ar", "Nav_Profiles", "الملفات الشخصية")]
    [InlineData("es", "Nav_Profiles", "Perfiles")]
    public void NavigationResources_ResolveTranslatedKeysForNewCultures(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<NavMenu>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
    }

    [Fact]
    public void SupportedCultures_ContainRequestedLanguages()
    {
        var supportedCultureCodes = SupportedCultures.Codes;

        Assert.Contains("tr", supportedCultureCodes);
        Assert.Contains("da", supportedCultureCodes);
        Assert.Contains("ar", supportedCultureCodes);
        Assert.Contains("ru", supportedCultureCodes);
        Assert.Contains("fr", supportedCultureCodes);
        Assert.Contains("zh", supportedCultureCodes);
        Assert.Contains("es", supportedCultureCodes);
        Assert.Contains("fa", supportedCultureCodes);
    }

    private static LocalizedString GetLocalizedString<TResource>(string culture, string key)
    {
        using var cultureScope = new CultureScope(culture);
        using var serviceProvider = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .AddLocalization(options => options.ResourcesPath = "Resources")
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IStringLocalizer<TResource>>()[key];
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string culture)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }
}
