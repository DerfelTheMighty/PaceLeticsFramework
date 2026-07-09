using PaceLetics.Web.Services.Achievements;

namespace PaceLetics.Tests;

public sealed class AchievementServiceTests
{
    [Fact]
    public async Task SetTrainingSessionCompletion_AwardsMatchingAchievementOnce()
    {
        var repository = new InMemoryAchievementRepository();
        var service = new AchievementService(repository);
        await service.SaveDefinitionAsync(new AchievementDefinitionRequest
        {
            Title = "Erste Einheit",
            Rules =
            {
                new AchievementRuleDocument
                {
                    RuleType = AchievementRuleTypes.TrainingSessionCount,
                    PlanId = "plan-1",
                    TargetCount = 1
                }
            }
        }, "trainer-1");

        var firstResult = await service.SetTrainingSessionCompletionAsync("athlete-1", "plan-1", "session-1", true);
        var secondResult = await service.SetTrainingSessionCompletionAsync("athlete-1", "plan-1", "session-1", true);

        Assert.Single(firstResult.Awarded);
        Assert.Empty(secondResult.Awarded);
        Assert.Single(await service.GetAwardsForAthleteAsync("athlete-1"));
    }

    [Fact]
    public async Task SetTrainingSessionCompletion_AwardPersistsAfterSessionIsUnchecked()
    {
        var repository = new InMemoryAchievementRepository();
        var service = new AchievementService(repository);
        await service.SaveDefinitionAsync(new AchievementDefinitionRequest
        {
            Title = "Dranbleiben",
            Rules =
            {
                new AchievementRuleDocument
                {
                    RuleType = AchievementRuleTypes.TrainingSessionCompleted,
                    PlanId = "plan-1",
                    SessionIds = new List<string> { "session-1" }
                }
            }
        }, "trainer-1");

        await service.SetTrainingSessionCompletionAsync("athlete-1", "plan-1", "session-1", true);
        await service.SetTrainingSessionCompletionAsync("athlete-1", "plan-1", "session-1", false);

        Assert.Empty(await service.GetTrainingSessionCompletionsForAthleteAsync("athlete-1"));
        var award = Assert.Single(await service.GetAwardsForAthleteAsync("athlete-1"));
        Assert.Equal("Dranbleiben", award.TitleSnapshot);
    }

    [Fact]
    public async Task TrainingSessionCountRule_WaitsForTargetCount()
    {
        var repository = new InMemoryAchievementRepository();
        var service = new AchievementService(repository);
        await service.SaveDefinitionAsync(new AchievementDefinitionRequest
        {
            Title = "Zwei Einheiten",
            Rules =
            {
                new AchievementRuleDocument
                {
                    RuleType = AchievementRuleTypes.TrainingSessionCount,
                    PlanId = "plan-1",
                    TargetCount = 2
                }
            }
        }, "trainer-1");

        var firstResult = await service.SetTrainingSessionCompletionAsync("athlete-1", "plan-1", "session-1", true);
        var secondResult = await service.SetTrainingSessionCompletionAsync("athlete-1", "plan-1", "session-2", true);

        Assert.Empty(firstResult.Awarded);
        var award = Assert.Single(secondResult.Awarded);
        Assert.Equal("Zwei Einheiten", award.TitleSnapshot);
    }

    [Fact]
    public async Task WorkoutCountRule_AwardsAfterEnoughCompletedWorkouts()
    {
        var repository = new InMemoryAchievementRepository();
        var service = new AchievementService(repository);
        await service.SaveDefinitionAsync(new AchievementDefinitionRequest
        {
            Title = "Workout-Serie",
            Rules =
            {
                new AchievementRuleDocument
                {
                    RuleType = AchievementRuleTypes.WorkoutCount,
                    WorkoutId = "core",
                    TargetCount = 2
                }
            }
        }, "trainer-1");

        var firstResult = await service.RecordWorkoutCompletedAsync("athlete-1", "core", "Core");
        var secondResult = await service.RecordWorkoutCompletedAsync("athlete-1", "core", "Core");

        Assert.Empty(firstResult.Awarded);
        var award = Assert.Single(secondResult.Awarded);
        Assert.Equal("Workout-Serie", award.TitleSnapshot);
    }

    private sealed class InMemoryAchievementRepository : IAchievementRepository
    {
        private readonly List<AchievementDefinitionDocument> _definitions = new();
        private readonly List<AthleteAchievementDocument> _awards = new();
        private readonly List<AchievementEventDocument> _events = new();
        private readonly List<TrainingSessionCompletionDocument> _completions = new();

        public Task<IReadOnlyList<AchievementDefinitionDocument>> GetDefinitionsAsync()
        {
            return Task.FromResult<IReadOnlyList<AchievementDefinitionDocument>>(
                _definitions.OrderBy(definition => definition.Title).ToList());
        }

        public Task<AchievementDefinitionDocument?> GetDefinitionAsync(string achievementDefinitionId)
        {
            return Task.FromResult(_definitions.FirstOrDefault(definition => definition.Id == achievementDefinitionId));
        }

        public Task UpsertDefinitionAsync(AchievementDefinitionDocument definition)
        {
            definition.Normalize();
            _definitions.RemoveAll(existing => existing.Id == definition.Id);
            _definitions.Add(definition);
            return Task.CompletedTask;
        }

        public Task DeleteDefinitionAsync(string achievementDefinitionId)
        {
            _definitions.RemoveAll(definition => definition.Id == achievementDefinitionId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AthleteAchievementDocument>> GetAwardsForAthleteAsync(string athleteUserId)
        {
            return Task.FromResult<IReadOnlyList<AthleteAchievementDocument>>(
                _awards.Where(award => award.AthleteUserId == athleteUserId).ToList());
        }

        public Task<AthleteAchievementDocument?> GetAwardAsync(string athleteUserId, string achievementDefinitionId)
        {
            return Task.FromResult(_awards.FirstOrDefault(award =>
                award.AthleteUserId == athleteUserId
                && award.AchievementDefinitionId == achievementDefinitionId));
        }

        public Task UpsertAwardAsync(AthleteAchievementDocument award)
        {
            award.Normalize();
            _awards.RemoveAll(existing => existing.Id == award.Id);
            _awards.Add(award);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AchievementEventDocument>> GetEventsForAthleteAsync(string athleteUserId)
        {
            return Task.FromResult<IReadOnlyList<AchievementEventDocument>>(
                _events.Where(achievementEvent => achievementEvent.AthleteUserId == athleteUserId).ToList());
        }

        public Task UpsertEventAsync(AchievementEventDocument achievementEvent)
        {
            achievementEvent.Normalize();
            _events.RemoveAll(existing => existing.Id == achievementEvent.Id);
            _events.Add(achievementEvent);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TrainingSessionCompletionDocument>> GetTrainingSessionCompletionsForAthleteAsync(string athleteUserId)
        {
            return Task.FromResult<IReadOnlyList<TrainingSessionCompletionDocument>>(
                _completions.Where(completion => completion.AthleteUserId == athleteUserId).ToList());
        }

        public Task<TrainingSessionCompletionDocument?> GetTrainingSessionCompletionAsync(
            string athleteUserId,
            string planId,
            string sessionId)
        {
            return Task.FromResult(_completions.FirstOrDefault(completion =>
                completion.AthleteUserId == athleteUserId
                && completion.PlanId == planId
                && completion.SessionId == sessionId));
        }

        public Task UpsertTrainingSessionCompletionAsync(TrainingSessionCompletionDocument completion)
        {
            completion.Normalize();
            _completions.RemoveAll(existing => existing.Id == completion.Id);
            _completions.Add(completion);
            return Task.CompletedTask;
        }

        public Task DeleteTrainingSessionCompletionAsync(string athleteUserId, string planId, string sessionId)
        {
            _completions.RemoveAll(completion =>
                completion.AthleteUserId == athleteUserId
                && completion.PlanId == planId
                && completion.SessionId == sessionId);
            return Task.CompletedTask;
        }
    }
}
