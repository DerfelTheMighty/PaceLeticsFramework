using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

namespace PaceLetics.Web.Services.Integrations.Strava;

public sealed class CosmosStravaIntegrationRepository : IStravaIntegrationRepository
{
    private readonly IDataAccess _data;
    private readonly AthleteDataOptions _options;

    public CosmosStravaIntegrationRepository(IDataAccess data, AthleteDataOptions options)
    {
        _data = data;
        _options = options;
        _options.Validate();
    }

    public Task<StravaConnectionDocument?> GetConnectionAsync(string athleteUserId, CancellationToken cancellationToken = default)
    {
        return _data.LoadItem<StravaConnectionDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            StravaDocumentIds.Connection(athleteUserId),
            StravaDocumentIds.Partition(athleteUserId),
            cancellationToken);
    }

    public Task UpsertConnectionAsync(StravaConnectionDocument connection, CancellationToken cancellationToken = default)
    {
        Normalize(connection);
        return _data.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            connection,
            connection.CourseId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<StravaActivityDocument>> GetActivitiesAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        var activities = await _data.LoadPartitionData<StravaActivityDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            StravaDocumentIds.Partition(athleteUserId),
            StravaDocumentTypes.Activity,
            cancellationToken);
        return activities.OrderByDescending(activity => activity.StartDateUtc).ToList();
    }

    public async Task UpsertActivitiesAsync(
        IReadOnlyCollection<StravaActivityDocument> activities,
        CancellationToken cancellationToken = default)
    {
        foreach (var activity in activities)
        {
            activity.CourseId = StravaDocumentIds.Partition(activity.AthleteUserId);
            activity.DocumentType = StravaDocumentTypes.Activity;
            activity.Id = StravaDocumentIds.Activity(activity.AthleteUserId, activity.StravaActivityId);
            await _data.UpsertItem(
                _options.DatabaseName,
                _options.CourseContainerName,
                activity,
                activity.CourseId,
                cancellationToken);
        }
    }

    public async Task DeleteAllAsync(string athleteUserId, CancellationToken cancellationToken = default)
    {
        var partition = StravaDocumentIds.Partition(athleteUserId);
        var activities = await GetActivitiesAsync(athleteUserId, cancellationToken);
        var ids = activities.Select(activity => activity.Id)
            .Append(StravaDocumentIds.Connection(athleteUserId))
            .ToList();
        await _data.DeleteItems(
            _options.DatabaseName,
            _options.CourseContainerName,
            ids,
            partition,
            cancellationToken);
    }

    private static void Normalize(StravaConnectionDocument connection)
    {
        connection.AthleteUserId = connection.AthleteUserId.Trim();
        connection.Id = StravaDocumentIds.Connection(connection.AthleteUserId);
        connection.CourseId = StravaDocumentIds.Partition(connection.AthleteUserId);
        connection.DocumentType = StravaDocumentTypes.Connection;
        connection.Scopes = connection.Scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(scope => scope)
            .ToList();
    }
}
