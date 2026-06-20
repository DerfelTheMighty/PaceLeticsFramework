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
        var (service, store) = CreateService();

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

        var persisted = Assert.Single(store.StoredCatalog!.Exercises, exercise => exercise.Id == "skip-drill");
        Assert.Equal("trainer-1", persisted.OwnerUserId);
        Assert.NotNull(persisted.CreatedAt);
        Assert.NotNull(persisted.UpdatedAt);
    }

    [Fact]
    public async Task UpsertWorkoutAsync_PersistsWorkoutForExistingExercises()
    {
        var (service, store) = CreateService();

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

        var persisted = Assert.Single(store.StoredCatalog!.Workouts, workout => workout.Id == "foundation-two");
        Assert.Equal("trainer-2", persisted.OwnerUserId);
        Assert.Equal(new[] { "glute-bridge" }, persisted.Exercises);
    }

    [Fact]
    public async Task EnsureLoadedAsync_AppliesPersistedCatalog()
    {
        var persistedCatalog = CreateCatalog();
        persistedCatalog.Exercises.Add(
            new ExerciseDefinition
            {
                Id = "ankle-hop",
                Name = "Ankle Hop",
                Description = "Elastic foot drill",
                Execution = new List<string> { "Hop lightly" },
                Duration = 20,
                Level = Level.Easy,
                Tags = new List<string> { "elasticity" },
                Source = "PaceLetics",
                OwnerUserId = "trainer-3"
            });
        var (service, store) = CreateService(persistedCatalog);

        await service.EnsureLoadedAsync();

        Assert.Equal(1, store.LoadCount);
        Assert.Contains(service.Catalog.Exercises, exercise => exercise.Id == "ankle-hop");
    }

    [Fact]
    public async Task UpsertWorkoutAsync_DoesNotMutateLiveCatalogWhenValidationFails()
    {
        var (service, store) = CreateService();
        var beforeCount = service.Catalog.Workouts.Count;
        var beforeSaveCount = store.SaveCount;

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
        Assert.Equal(beforeSaveCount, store.SaveCount);
        Assert.DoesNotContain(service.Catalog.Workouts, workout => workout.Id == "invalid-workout");
    }

    private static (WorkoutCatalogManagementService Service, InMemoryWorkoutCatalogStore Store) CreateService(
        WorkoutCatalogDocument? storedCatalog = null)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var repository = new JsonWorkoutCatalogRepository(path);
        var catalog = CreateCatalog();
        repository.Save(catalog);
        var loadedCatalog = repository.Load();
        var store = new InMemoryWorkoutCatalogStore(storedCatalog ?? loadedCatalog);
        var service = new WorkoutCatalogManagementService(store, repository, loadedCatalog);

        return (service, store);
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

    private static WorkoutCatalogDocument CloneCatalog(WorkoutCatalogDocument source)
    {
        return new WorkoutCatalogDocument
        {
            SchemaVersion = source.SchemaVersion,
            Exercises = source.Exercises.Select(CloneExercise).ToList(),
            Workouts = source.Workouts.Select(CloneWorkout).ToList()
        };
    }

    private static ExerciseDefinition CloneExercise(ExerciseDefinition source)
    {
        return new ExerciseDefinition
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Execution = source.Execution.ToList(),
            Duration = source.Duration,
            ImageFile = source.ImageFile,
            Level = source.Level,
            SwitchLeftRight = source.SwitchLeftRight,
            SwitchTime = source.SwitchTime,
            Tags = source.Tags.ToList(),
            ReadMore = source.ReadMore.Select(CloneReference).ToList(),
            Source = source.Source,
            OwnerUserId = source.OwnerUserId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static WorkoutDefinition CloneWorkout(WorkoutDefinition source)
    {
        return new WorkoutDefinition
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Level = source.Level,
            PreparationTime = source.PreparationTime,
            RestTime = source.RestTime,
            SwitchTime = source.SwitchTime,
            Exercises = source.Exercises.ToList(),
            Tags = source.Tags.ToList(),
            ReadMore = source.ReadMore.Select(CloneReference).ToList(),
            Source = source.Source,
            OwnerUserId = source.OwnerUserId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static ContentReference CloneReference(ContentReference source)
    {
        return new ContentReference
        {
            Title = source.Title,
            Url = source.Url,
            Description = source.Description,
            SourceType = source.SourceType
        };
    }

    private sealed class InMemoryWorkoutCatalogStore : IWorkoutCatalogStore
    {
        public InMemoryWorkoutCatalogStore(WorkoutCatalogDocument? storedCatalog)
        {
            StoredCatalog = storedCatalog is null ? null : CloneCatalog(storedCatalog);
        }

        public WorkoutCatalogDocument? StoredCatalog { get; private set; }
        public int LoadCount { get; private set; }
        public int SaveCount { get; private set; }

        public Task<WorkoutCatalogDocument> LoadOrSeedAsync(WorkoutCatalogDocument seedCatalog)
        {
            LoadCount++;
            StoredCatalog ??= CloneCatalog(seedCatalog);
            return Task.FromResult(CloneCatalog(StoredCatalog));
        }

        public Task SaveAsync(WorkoutCatalogDocument catalog)
        {
            SaveCount++;
            StoredCatalog = CloneCatalog(catalog);
            return Task.CompletedTask;
        }
    }
}
