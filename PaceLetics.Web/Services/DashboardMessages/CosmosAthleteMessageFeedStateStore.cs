using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class CosmosAthleteMessageFeedStateStore : IAthleteMessageFeedStateStore
{
    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;
    private readonly TimeProvider _timeProvider;

    public CosmosAthleteMessageFeedStateStore(IDataAccess db, AthleteDataOptions options, TimeProvider? timeProvider = null)
    {
        _db = db;
        _options = options;
        _timeProvider = timeProvider ?? TimeProvider.System;
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
        state.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            state,
            state.CourseId);
    }
}
