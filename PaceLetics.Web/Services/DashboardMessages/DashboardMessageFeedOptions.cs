namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class DashboardMessageFeedOptions
{
    public TimeSpan ReferenceRunMaxAge { get; init; } = TimeSpan.FromDays(180);

    public TimeSpan UpcomingTrainingLookAhead { get; init; } = TimeSpan.FromDays(14);
}
