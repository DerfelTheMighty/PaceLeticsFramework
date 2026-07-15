using PaceLetics.Web.Services.TrainingFeedback;

namespace PaceLetics.Tests;

public sealed class TrainingFeedbackServiceTests
{
    [Fact]
    public async Task SaveAsync_NormalizesAndPersistsSessionFeedback()
    {
        var repository = new InMemoryTrainingFeedbackRepository();
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 7, 15, 8, 0, 0, TimeSpan.Zero));
        var service = new TrainingFeedbackService(repository, clock);

        var saved = await service.SaveAsync(
            " athlete-1 ",
            new TrainingFeedbackInput(7, TrainingFeeling.Good, "  Felt controlled.  "),
            " Tempo run ",
            "planned-session",
            "plan-1",
            "session-1");

        Assert.Single(repository.Items);
        Assert.Equal("athlete-1", saved.AthleteUserId);
        Assert.Equal("Felt controlled.", saved.Comment);
        Assert.Equal(clock.GetUtcNow().UtcDateTime, saved.CompletedAt);
        Assert.Contains("session:athlete-1:plan-1:session-1", saved.Id);
    }

    [Fact]
    public async Task GetRecommendationAsync_RecommendsRecoveryAfterVeryHardFeedback()
    {
        var now = new DateTimeOffset(2026, 7, 15, 8, 0, 0, TimeSpan.Zero);
        var repository = new InMemoryTrainingFeedbackRepository(
            Feedback("athlete-1", effort: 9, TrainingFeeling.Poor, now.UtcDateTime.AddHours(-2)));
        var service = new TrainingFeedbackService(repository, new FixedTimeProvider(now));

        var recommendation = await service.GetRecommendationAsync("athlete-1", hasUpcomingTraining: true);

        Assert.Equal(TrainingRecommendationKind.PrioritizeRecovery, recommendation.Kind);
        Assert.Equal(1, recommendation.FeedbackCount);
    }

    [Fact]
    public async Task GetRecommendationAsync_RecommendsEasyTrainingAfterTwoHardSessions()
    {
        var now = new DateTimeOffset(2026, 7, 15, 8, 0, 0, TimeSpan.Zero);
        var repository = new InMemoryTrainingFeedbackRepository(
            Feedback("athlete-1", effort: 8, TrainingFeeling.Good, now.UtcDateTime.AddHours(-2)),
            Feedback("athlete-1", effort: 8, TrainingFeeling.Neutral, now.UtcDateTime.AddDays(-1)));
        var service = new TrainingFeedbackService(repository, new FixedTimeProvider(now));

        var recommendation = await service.GetRecommendationAsync("athlete-1", hasUpcomingTraining: true);

        Assert.Equal(TrainingRecommendationKind.TakeItEasy, recommendation.Kind);
    }

    [Fact]
    public async Task GetRecommendationAsync_UsesUpcomingStateWithoutFeedback()
    {
        var service = new TrainingFeedbackService(
            new InMemoryTrainingFeedbackRepository(),
            new FixedTimeProvider(DateTimeOffset.UtcNow));

        var recommendation = await service.GetRecommendationAsync("athlete-1", hasUpcomingTraining: true);

        Assert.Equal(TrainingRecommendationKind.Upcoming, recommendation.Kind);
    }

    private static TrainingFeedbackDocument Feedback(
        string athleteUserId,
        int effort,
        TrainingFeeling feeling,
        DateTime completedAt)
    {
        var feedback = new TrainingFeedbackDocument
        {
            AthleteUserId = athleteUserId,
            Effort = effort,
            Feeling = feeling,
            CompletedAt = completedAt
        };
        feedback.Normalize();
        return feedback;
    }

    private sealed class InMemoryTrainingFeedbackRepository : ITrainingFeedbackRepository
    {
        public InMemoryTrainingFeedbackRepository(params TrainingFeedbackDocument[] items)
        {
            Items.AddRange(items);
        }

        public List<TrainingFeedbackDocument> Items { get; } = new();

        public Task<IReadOnlyList<TrainingFeedbackDocument>> GetForAthleteAsync(string athleteUserId)
        {
            IReadOnlyList<TrainingFeedbackDocument> result = Items
                .Where(item => string.Equals(item.AthleteUserId, athleteUserId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.CompletedAt)
                .ToList();
            return Task.FromResult(result);
        }

        public Task UpsertAsync(TrainingFeedbackDocument feedback)
        {
            var index = Items.FindIndex(item => item.Id == feedback.Id);
            if (index >= 0)
                Items[index] = feedback;
            else
                Items.Add(feedback);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
