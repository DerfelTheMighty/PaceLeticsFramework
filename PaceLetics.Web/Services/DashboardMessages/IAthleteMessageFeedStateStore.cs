namespace PaceLetics.Web.Services.DashboardMessages;

public interface IAthleteMessageFeedStateStore
{
    Task<AthleteMessageFeedStateDocument> GetAsync(string athleteUserId);

    Task SaveAsync(AthleteMessageFeedStateDocument state);
}
