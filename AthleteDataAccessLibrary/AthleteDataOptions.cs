namespace AthleteDataAccessLibrary;

public sealed class AthleteDataOptions
{
    public const string SectionName = "AthleteData";

    public string DatabaseName { get; set; } = "paceleticsdata";

    public string AthleteContainerName { get; set; } = "athletedata";

    public string CourseContainerName { get; set; } = "courses";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DatabaseName))
            throw new InvalidOperationException("AthleteData:DatabaseName must be configured.");

        if (string.IsNullOrWhiteSpace(AthleteContainerName))
            throw new InvalidOperationException("AthleteData:AthleteContainerName must be configured.");

        if (string.IsNullOrWhiteSpace(CourseContainerName))
            throw new InvalidOperationException("AthleteData:CourseContainerName must be configured.");

    }
}
