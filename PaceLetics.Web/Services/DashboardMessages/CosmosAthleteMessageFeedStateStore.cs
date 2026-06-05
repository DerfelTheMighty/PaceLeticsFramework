using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class CosmosAthleteMessageFeedStateStore : IAthleteMessageFeedStateStore
{
    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;

    public CosmosAthleteMessageFeedStateStore(IDataAccess db, AthleteDataOptions options)
    {
        _db = db;
        _options = options;
        _options.Validate();
    }

    public async Task<AthleteMessageFeedStateDocument> GetAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return AthleteMessageFeedStateDocument.Create(string.Empty);

        var state = await _db.LoadItem<AthleteMessageFeedStateDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            AthleteMessageFeedStateIds.Document(athleteUserId),
            AthleteMessageFeedStateIds.Partition(athleteUserId));

        state ??= AthleteMessageFeedStateDocument.Create(athleteUserId);
        state.AthleteUserId = athleteUserId;
        state.Normalize();

        return state;
    }

    public Task SaveAsync(AthleteMessageFeedStateDocument state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Normalize();
        state.UpdatedAt = DateTime.UtcNow;

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            state,
            state.CourseId);
    }
}
