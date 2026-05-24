using AthleteDataAccessLibrary;

namespace PaceLetics.Web.Configuration;

public static class PaceLeticsConfiguration
{
    public static string GetRequiredEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"Environment variable '{name}' is not configured.");
    }

    public static AthleteDataOptions GetAthleteDataOptions(this IConfiguration configuration)
    {
        var defaults = new AthleteDataOptions();
        var section = configuration.GetSection(AthleteDataOptions.SectionName);
        var options = new AthleteDataOptions
        {
            DatabaseName = section[nameof(AthleteDataOptions.DatabaseName)] ?? defaults.DatabaseName,
            AthleteContainerName = section[nameof(AthleteDataOptions.AthleteContainerName)] ?? defaults.AthleteContainerName
        };

        options.Validate();
        return options;
    }
}
