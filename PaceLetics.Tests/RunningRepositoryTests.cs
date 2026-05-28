using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;

namespace PaceLetics.Tests;

public sealed class RunningRepositoryTests
{
    [Fact]
    public void JsonRunningSessionRepository_LoadsDefinitionsWithoutCreatingRuntimeSessions()
    {
        var path = WriteTempJson("""
        [
          {
            "sessionType": "planned",
            "id": "planned-test",
            "name": "Planned Test",
            "date": "2026-01-01",
            "sequence": [
              { "type": "Dauerlauf", "distance": 1000, "paceKey": "E Pace" }
            ]
          }
        ]
        """);

        var definitions = new JsonRunningSessionRepository(path).Load();

        var definition = Assert.IsType<PlannedSessionDto>(Assert.Single(definitions));
        Assert.Equal("planned-test", definition.Id);
    }

    [Fact]
    public void RunningSessionFactory_CreatesRuntimeSessionFromDefinition()
    {
        var definition = new PlannedSessionDto
        {
            Id = "planned-test",
            Name = "Planned Test",
            Date = new DateTime(2026, 1, 1),
            Sequence =
            [
                new RunningSegmentDto
                {
                    Type = SegmentType.Dauerlauf,
                    Distance = 1000,
                    PaceKey = "E Pace"
                }
            ]
        };

        var session = RunningSessionFactory.Create(definition);

        Assert.Equal("planned-test", session.Id);
        Assert.Equal(1000, session.TotalDistance);
    }

    [Fact]
    public void JsonTrainingPlanRepository_LoadsCommittedPlanFiles()
    {
        var plansDirectory = Path.Combine(
            FindRepositoryRoot(),
            "PaceLetics.Web",
            "wwwroot",
            "data",
            "plans");

        var plans = new JsonTrainingPlanRepository(plansDirectory).Load();

        Assert.NotEmpty(plans);
        Assert.All(plans, plan => Assert.NotEmpty(plan.Sessions));
    }

    [Fact]
    public void JsonTrainingPlanRepository_LoadsTrainingSessionsWithRunsAndWorkouts()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "mixed-plan.json"), """
        {
          "schemaVersion": 1,
          "id": "mixed-plan",
          "name": "Mixed Plan",
          "sessions": [
            {
              "id": "mixed-session",
              "name": "Run and Workout",
              "date": "2026-01-01",
              "runs": [
                {
                  "sessionType": "planned",
                  "id": "planned-run",
                  "name": "Easy Run",
                  "date": "2026-01-01",
                  "sequence": [
                    { "type": "Dauerlauf", "distance": 1000, "paceKey": "E Pace" }
                  ]
                }
              ],
              "workouts": [
                { "workoutId": "Stabi Handout Easy", "sets": 2, "rounds": 3 }
              ]
            }
          ]
        }
        """);

        var plans = new JsonTrainingPlanRepository(directory, WorkoutCatalogTestData.CreateWorkoutCatalog()).Load();

        var plan = Assert.Single(plans);
        var session = Assert.Single(plan.Sessions);
        Assert.Equal("mixed-plan", plan.Id);
        Assert.Equal("planned-run", Assert.Single(session.Runs).Id);
        Assert.Equal("Stabi Handout Easy", Assert.Single(session.Workouts).WorkoutId);
        Assert.Equal(1000, plan.TotalRunDistance);
    }

    [Fact]
    public void JsonTrainingPlanRepository_ValidatesWorkoutReferences()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "invalid-plan.json"), """
        {
          "id": "invalid-plan",
          "name": "Invalid Plan",
          "sessions": [
            {
              "id": "invalid-session",
              "name": "Invalid Session",
              "date": "2026-01-01",
              "workouts": [
                { "workoutId": "missing-workout", "sets": 1, "rounds": 1 }
              ]
            }
          ]
        }
        """);

        Assert.Throws<InvalidDataException>(() =>
            new JsonTrainingPlanRepository(directory, WorkoutCatalogTestData.CreateWorkoutCatalog()).Load());
    }

    private static string WriteTempJson(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PaceLeticsFramework.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate PaceLeticsFramework.sln.");
    }
}
