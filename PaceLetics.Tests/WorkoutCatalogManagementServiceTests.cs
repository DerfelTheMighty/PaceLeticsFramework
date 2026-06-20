using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.TrainingModule.CodeBase.Workouts.Repositories;
using PaceLetics.Web.Services.Workouts;

namespace PaceLetics.Tests;

public sealed class WorkoutCatalogManagementServiceTests
{
    [Fact]
    public async Task UpsertExerciseAsync_PersistsExerciseAndUpdatesLiveCatalog()
    {
        var (service, repository) = CreateService();

        await service.UpsertExerciseAsync(
            new ExerciseDefinition
            {
                Id = "skip-drill",
                Name = "Skip Drill",
                Description = "Fast coordination drill",
                Execution = new List<string> { "Stand tall", "Drive the knee" },
                Duration = 30,
                ImageFile = "skip_drill.png",
                Level = Level.Easy,
                Tags = new List<string> { "coordination", "running" },
                Source = "PaceLetics",
                ReadMore =
                {
                    new ContentReference { Title = "Drill note", Url = "/academy/drills" }
                }
            },
            "trainer-1");

        Assert.Contains(service.Catalog.Exercises, exercise => exercise.Id == "skip-drill");

        var reloaded = repository.Load();
        var persisted = Assert.Single(reloaded.Exercises, exercise => exercise.Id == "skip-drill");
        Assert.Equal("trainer-1", persisted.OwnerUserId);
        Assert.NotNull(persisted.CreatedAt);
        Assert.NotNull(persisted.UpdatedAt);
    }

    [Fact]
    public async Task UpsertWorkoutAsync_PersistsWorkoutForExistingExercises()
    {
        var (service, repository) = CreateService();

        await service.UpsertWorkoutAsync(
            new WorkoutDefinition
            {
                Id = "foundation-two",
                Name = "Foundation Two",
                Description = "Two exercise foundation workout",
                Level = Level.Easy,
                PreparationTime = 10,
                RestTime = 15,
                SwitchTime = 5,
                Exercises = new List<string> { "glute-bridge" },
                Tags = new List<string> { "foundation" },
                ReadMore =
                {
                    new ContentReference { Title = "Workout context", Url = "/academy/foundation" }
                }
            },
            "trainer-2");

        Assert.Contains(service.Catalog.Workouts, workout => workout.Id == "foundation-two");

        var reloaded = repository.Load();
        var persisted = Assert.Single(reloaded.Workouts, workout => workout.Id == "foundation-two");
        Assert.Equal("trainer-2", persisted.OwnerUserId);
        Assert.Equal(new[] { "glute-bridge" }, persisted.Exercises);
    }

    [Fact]
    public async Task UpsertWorkoutAsync_DoesNotMutateLiveCatalogWhenValidationFails()
    {
        var (service, _) = CreateService();
        var beforeCount = service.Catalog.Workouts.Count;

        await Assert.ThrowsAsync<WorkoutCatalogValidationException>(() =>
            service.UpsertWorkoutAsync(
                new WorkoutDefinition
                {
                    Id = "invalid-workout",
                    Name = "Invalid Workout",
                    Level = Level.Easy,
                    PreparationTime = 10,
                    RestTime = 15,
                    Exercises = new List<string> { "missing-exercise" }
                },
                "trainer-1"));

        Assert.Equal(beforeCount, service.Catalog.Workouts.Count);
        Assert.DoesNotContain(service.Catalog.Workouts, workout => workout.Id == "invalid-workout");
    }

    private static (WorkoutCatalogManagementService Service, JsonWorkoutCatalogRepository Repository) CreateService()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var repository = new JsonWorkoutCatalogRepository(path);
        var catalog = CreateCatalog();
        repository.Save(catalog);
        var loadedCatalog = repository.Load();
        var service = new WorkoutCatalogManagementService(repository, loadedCatalog);

        return (service, repository);
    }

    private static WorkoutCatalogDocument CreateCatalog()
    {
        return new WorkoutCatalogDocument
        {
            SchemaVersion = 1,
            Exercises =
            {
                new ExerciseDefinition
                {
                    Id = "glute-bridge",
                    Name = "Glute Bridge",
                    Description = "Base exercise",
                    Execution = new List<string> { "Start", "Hold" },
                    Duration = 30,
                    Level = Level.Easy,
                    Tags = new List<string> { "stability" },
                    Source = "PaceLetics",
                    OwnerUserId = "seed",
                    ReadMore =
                    {
                        new ContentReference { Title = "Reference", Url = "/academy/glute-bridge" }
                    }
                }
            },
            Workouts =
            {
                new WorkoutDefinition
                {
                    Id = "foundation-one",
                    Name = "Foundation One",
                    Description = "Base workout",
                    Level = Level.Easy,
                    PreparationTime = 10,
                    RestTime = 15,
                    SwitchTime = 5,
                    Exercises = new List<string> { "glute-bridge" },
                    Tags = new List<string> { "foundation" },
                    Source = "PaceLetics",
                    OwnerUserId = "seed",
                    ReadMore =
                    {
                        new ContentReference { Title = "Reference", Url = "/academy/foundation" }
                    }
                }
            }
        };
    }
}
