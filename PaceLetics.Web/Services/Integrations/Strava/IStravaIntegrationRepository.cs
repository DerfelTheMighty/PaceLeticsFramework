namespace PaceLetics.Web.Services.Integrations.Strava;

public interface IStravaIntegrationRepository
{
    Task<StravaConnectionDocument?> GetConnectionAsync(string athleteUserId, CancellationToken cancellationToken = default);
    Task UpsertConnectionAsync(StravaConnectionDocument connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StravaActivityDocument>> GetActivitiesAsync(string athleteUserId, CancellationToken cancellationToken = default);
    Task UpsertActivitiesAsync(IReadOnlyCollection<StravaActivityDocument> activities, CancellationToken cancellationToken = default);
    Task DeleteAllAsync(string athleteUserId, CancellationToken cancellationToken = default);
}
