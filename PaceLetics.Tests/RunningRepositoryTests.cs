using PaceLetics.RunningModule.CodeBase.Models;
using PaceLetics.RunningModule.CodeBase.Repositories;

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

    private static string WriteTempJson(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
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
