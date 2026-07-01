using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingModule.CodeBase.Running.Services;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Services;

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

        var definition = Assert.IsType<PlannedSessionDefinition>(Assert.Single(definitions));
        Assert.Equal("planned-test", definition.Id);
    }

    [Fact]
    public void RunningSessionFactory_CreatesRuntimeSessionFromDefinition()
    {
        var definition = new PlannedSessionDefinition
        {
            Id = "planned-test",
            Name = "Planned Test",
            Date = new DateTime(2026, 1, 1),
            Sequence =
            [
                new RunningSegmentDefinition
                {
                    Type = SegmentType.Dauerlauf,
                    Distance = 1000,
                    PaceKey = "E Pace"
                }
            ]
        };

        var session = new RunningSessionFactory().Create(definition);

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

        var plans = LoadTrainingPlans(directory);

        var plan = Assert.Single(plans);
        var session = Assert.Single(plan.Sessions);
        Assert.Equal("mixed-plan", plan.Id);
        Assert.Equal("planned-run", Assert.Single(session.Runs).Id);
        Assert.Equal("Stabi Handout Easy", Assert.Single(session.Workouts).WorkoutId);
        Assert.Equal(1000, plan.TotalRunDistance);
    }

    [Fact]
    public void JsonTrainingPlanRepository_LoadsTrainingSessionPreparationAndEffectMetadata()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "structured-plan.json"), """
        {
          "schemaVersion": 1,
          "id": "structured-plan",
          "name": "Structured Plan",
          "sessions": [
            {
              "id": "structured-session",
              "name": "Track Session",
              "date": "2026-01-01",
              "appointment": {
                "startsAt": "2026-01-01T18:00:00",
                "endsAt": "2026-01-01T19:30:00",
                "location": "Track",
                "notes": "Bring spikes"
              },
              "trainingEffect": {
                "focus": "Technique",
                "stimulus": "Coordination under light fatigue",
                "adaptation": "Cleaner foot placement",
                "recovery": "Easy next day"
              },
              "warmup": [
                { "title": "Mobility", "description": "Dynamic mobility", "durationSeconds": 600 }
              ],
              "drills": [
                { "title": "A-skip", "activityType": "drill", "durationSeconds": 300, "referenceId": "askip" }
              ],
              "workouts": [
                { "workoutId": "Stabi Handout Easy", "sets": 1, "rounds": 1 }
              ]
            }
          ]
        }
        """);

        var plans = LoadTrainingPlans(directory);

        var session = Assert.Single(Assert.Single(plans).Sessions);
        Assert.True(session.HasPreparation);
        Assert.True(session.HasTrainingEffect);
        Assert.True(session.HasAppointment);
        Assert.Equal("Mobility", Assert.Single(session.Warmup).Title);
        Assert.Equal("A-skip", Assert.Single(session.Drills).Title);
        Assert.Equal("Technique", session.TrainingEffect.Focus);
        Assert.Equal("Track", session.Appointment.Location);
        Assert.Equal(new DateTime(2026, 1, 1, 18, 0, 0), session.Appointment.StartsAt);
    }

    [Fact]
    public void JsonTrainingPlanRepository_LoadsTrainingPlanBlocks()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "blocked-plan.json"), """
        {
          "schemaVersion": 2,
          "id": "blocked-plan",
          "name": "Blocked Plan",
          "blocks": [
            {
              "id": "prep",
              "name": "Vorbereitung",
              "focus": "Technikgrundlagen",
              "structure": "3 Aufbauwochen + 1 Entlastungswoche",
              "order": 1,
              "sessionIds": [ "session-1", "session-2" ]
            }
          ],
          "sessions": [
            {
              "id": "session-1",
              "name": "Easy Run",
              "date": "2026-01-01",
              "runs": [
                {
                  "sessionType": "planned",
                  "id": "run-1",
                  "name": "Easy Run",
                  "date": "2026-01-01",
                  "sequence": [
                    { "type": "Dauerlauf", "distance": 1000, "paceKey": "E Pace" }
                  ]
                }
              ],
              "workouts": []
            },
            {
              "id": "session-2",
              "name": "Strength",
              "date": "2026-01-08",
              "workouts": [
                { "workoutId": "Stabi Handout Easy", "sets": 1, "rounds": 1 }
              ]
            }
          ]
        }
        """);

        var plans = LoadTrainingPlans(directory);

        var plan = Assert.Single(plans);
        var block = Assert.Single(plan.Blocks);
        Assert.Equal("prep", block.Id);
        Assert.Equal("Technikgrundlagen", block.Focus);
        Assert.Equal(["session-1", "session-2"], block.SessionIds);
        Assert.Equal(2, plan.GetSessionsForBlock(block).Count);
        Assert.Same(block, plan.GetBlockForSession(plan.Sessions[0]));
    }

    [Fact]
    public void JsonTrainingPlanRepository_SavesTrainingPlanBlocks()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "save-blocks-plan.json"), """
        {
          "schemaVersion": 1,
          "id": "save-blocks-plan",
          "name": "Save Blocks Plan",
          "sessions": [
            {
              "id": "session-1",
              "name": "Strength",
              "date": "2026-01-01",
              "workouts": [
                { "workoutId": "Stabi Handout Easy", "sets": 1, "rounds": 1 }
              ]
            }
          ]
        }
        """);
        var repository = new JsonTrainingPlanRepository(directory);
        var definition = Assert.Single(repository.Load());
        definition.Blocks =
        [
            new TrainingPlanBlockDefinition
            {
                Id = "strength",
                Name = "Koordination und Kraft",
                Focus = "Stabilitaet",
                Order = 1,
                SessionIds = [ "session-1" ]
            }
        ];

        repository.Save(definition);

        var reloaded = Assert.Single(repository.Load());
        var block = Assert.Single(reloaded.Blocks);
        Assert.Equal(2, reloaded.SchemaVersion);
        Assert.Equal("strength", block.Id);
        Assert.Equal("session-1", Assert.Single(block.SessionIds));
    }

    [Fact]
    public void JsonTrainingPlanRepository_UsesAppointmentStartDateWhenSessionDateIsMissing()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "appointment-plan.json"), """
        {
          "schemaVersion": 1,
          "id": "appointment-plan",
          "name": "Appointment Plan",
          "sessions": [
            {
              "name": "Workout Appointment",
              "appointment": {
                "startsAt": "2026-02-03T17:30:00"
              },
              "workouts": [
                { "workoutId": "Stabi Handout Easy", "sets": 1, "rounds": 1 }
              ]
            }
          ]
        }
        """);

        var plans = LoadTrainingPlans(directory);

        var session = Assert.Single(Assert.Single(plans).Sessions);
        Assert.Equal(new DateTime(2026, 2, 3), session.Date);
        Assert.Equal("2026-02-03-workout-appointment", session.Id);
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
            LoadTrainingPlans(directory));
    }

    [Fact]
    public void JsonTrainingPlanRepository_ValidatesPreparationDurations()
    {
        var directory = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directory, "invalid-duration-plan.json"), """
        {
          "id": "invalid-duration-plan",
          "name": "Invalid Duration Plan",
          "sessions": [
            {
              "id": "invalid-duration-session",
              "name": "Invalid Duration Session",
              "date": "2026-01-01",
              "warmup": [
                { "title": "Mobility", "durationSeconds": -1 }
              ],
              "workouts": [
                { "workoutId": "Stabi Handout Easy", "sets": 1, "rounds": 1 }
              ]
            }
          ]
        }
        """);

        Assert.Throws<InvalidDataException>(() =>
            LoadTrainingPlans(directory));
    }

    [Fact]
    public void TrainingPlanDefinitionValidator_ReturnsErrorsForInvalidDefinition()
    {
        var definition = new TrainingPlanDefinition
        {
            Id = "invalid-plan",
            Name = "Invalid Plan",
            Sessions =
            [
                new TrainingSessionDefinition
                {
                    Id = "invalid-session",
                    Name = "Invalid Session",
                    Workouts =
                    [
                        new("missing-workout", Sets: 0, Rounds: 0)
                    ],
                    Warmup =
                    [
                        new TrainingPlanModule.CodeBase.Models.TrainingSessionActivity(
                            Title: "Mobility",
                            DurationSeconds: -1)
                    ]
                }
            ]
        };

        var validator = new TrainingPlanDefinitionValidator(WorkoutCatalogTestData.CreateWorkoutCatalog());

        var ex = Assert.Throws<TrainingPlanDefinitionValidationException>(() => validator.Validate(definition));
        Assert.Contains(ex.Errors, error => error.Contains("must define a date"));
        Assert.Contains(ex.Errors, error => error.Contains("references an unknown workout"));
        Assert.Contains(ex.Errors, error => error.Contains("sets must be greater than zero"));
        Assert.Contains(ex.Errors, error => error.Contains("duration must not be negative"));
    }

    private static string WriteTempJson(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static IReadOnlyList<TrainingPlanModule.CodeBase.Models.TrainingPlan> LoadTrainingPlans(string directory)
    {
        var definitions = new JsonTrainingPlanRepository(directory).Load();
        return new TrainingPlanFactory(
            new RunningSessionFactory(),
            WorkoutCatalogTestData.CreateWorkoutCatalog()).Create(definitions);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string FindRepositoryRoot(
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        var sourceDirectory = string.IsNullOrWhiteSpace(sourceFilePath)
            ? null
            : new DirectoryInfo(Path.GetDirectoryName(sourceFilePath)!);
        var directory = FindRepositoryRootFrom(sourceDirectory)
            ?? FindRepositoryRootFrom(new DirectoryInfo(Directory.GetCurrentDirectory()))
            ?? FindRepositoryRootFrom(new DirectoryInfo(AppContext.BaseDirectory));

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate PaceLeticsFramework.sln.");
    }

    private static DirectoryInfo? FindRepositoryRootFrom(DirectoryInfo? directory)
    {
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "PaceLeticsFramework.sln")))
        {
            directory = directory.Parent;
        }

        return directory;
    }
}
