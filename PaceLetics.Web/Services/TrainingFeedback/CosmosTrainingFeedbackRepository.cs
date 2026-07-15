using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

namespace PaceLetics.Web.Services.TrainingFeedback;

public sealed class CosmosTrainingFeedbackRepository : ITrainingFeedbackRepository
{
    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;

    public CosmosTrainingFeedbackRepository(IDataAccess db, AthleteDataOptions options)
    {
        _db = db;
        _options = options;
        _options.Validate();
    }

    public async Task<IReadOnlyList<TrainingFeedbackDocument>> GetForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<TrainingFeedbackDocument>();

        var feedback = await _db.LoadPartitionData<TrainingFeedbackDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            TrainingFeedbackDocumentIds.Partition(athleteUserId),
            TrainingFeedbackDocumentTypes.Feedback);

        foreach (var item in feedback)
            item.Normalize();

        return feedback
            .OrderByDescending(item => item.CompletedAt)
            .ToList();
    }

    public Task UpsertAsync(TrainingFeedbackDocument feedback)
    {
        feedback.Normalize();
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            feedback,
            feedback.CourseId);
    }
}
