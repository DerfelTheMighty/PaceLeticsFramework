using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingModule.CodeBase.Running.Services;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Services;
using PaceLetics.Web.Services;

namespace PaceLetics.Tests;

public sealed class TrainingPlanServiceTests
{
    [Fact]
    public void TrainingPlanService_CreatesPlanAndMaintainsSessionsAndBlocks()
    {
        var directory = CreateTempDirectory();
        var repository = new JsonTrainingPlanRepository(directory);
        var runningSessionFactory = new RunningSessionFactory();
        var service = new TrainingPlanService(
            repository,
            new TrainingPlanFactory(
                runningSessionFactory,
                WorkoutCatalogTestData.CreateWorkoutCatalog()),
            new EmptyRunningSessionRepository(),
            runningSessionFactory,
            null!);

        var plan = service.CreateTrainingPlan("10K Aufbau");

        Assert.Equal("10k-aufbau", plan.Id);
        Assert.Empty(plan.Sessions);

        plan = service.AddTrainingSession(plan.Id, CreatePlannedSession("Easy Run"));
        var session = Assert.Single(plan.Sessions);

        plan = service.AddTrainingPlanBlock(
            plan.Id,
            new TrainingPlanBlockDefinition
            {
                Name = "Grundlage",
                Focus = "Aerobe Basis",
                SessionIds = [session.Id]
            });

        var block = Assert.Single(plan.Blocks);
        Assert.Equal(session.Id, Assert.Single(block.SessionIds));

        plan = service.RemoveTrainingSession(plan.Id, session.Id);

        Assert.Empty(plan.Sessions);
        Assert.Empty(plan.Blocks);
        Assert.Empty(Assert.Single(repository.Load()).Blocks);
    }

    private static TrainingSessionDefinition CreatePlannedSession(string name)
    {
        var date = new DateTime(2026, 1, 1);
        return new TrainingSessionDefinition
        {
            Name = name,
            Date = date,
            Runs =
            [
                new PlannedSessionDefinition
                {
                    Name = name,
                    Date = date,
                    Sequence =
                    [
                        new RunningSegmentDefinition
                        {
                            Type = SegmentType.Dauerlauf,
                            Distance = 5_000,
                            PaceKey = PaceKeys.Easy
                        }
                    ]
                }
            ]
        };
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private sealed class EmptyRunningSessionRepository : IRunningSessionRepository
    {
        public IReadOnlyList<RunningSessionDefinition> Load()
        {
            return Array.Empty<RunningSessionDefinition>();
        }
    }
}
