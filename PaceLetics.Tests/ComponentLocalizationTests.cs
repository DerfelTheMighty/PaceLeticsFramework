using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PaceLetics.TrainingModule.Components.Running;
using PaceLetics.TrainingModule.Components.Workouts;

namespace PaceLetics.Tests;

public sealed class ComponentLocalizationTests
{
    [Theory]
    [InlineData("de", "DifficultyDialog_Start", "Starten")]
    [InlineData("en", "DifficultyDialog_Start", "Start")]
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
    public void RunningResources_ResolveKnownKeysWithAppResourcePath(
        string culture,
        string key,
        string expectedValue)
    {
        var value = GetLocalizedString<RunningResources>(culture, key);

        Assert.False(value.ResourceNotFound);
        Assert.Equal(expectedValue, value.Value);
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
