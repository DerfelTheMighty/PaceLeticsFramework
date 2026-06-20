using PaceLetics.TrainingModule.CodeBase.Workouts.Services;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;

namespace PaceLetics.Tests;

public sealed class WorkoutCatalogServiceTests
{
    [Fact]
    public void WorkoutCatalog_GroupsWorkoutVariantsByBaseName()
    {
        var catalog = CreateCatalog();

        var ids = catalog.GetWorkoutIdsByName("Stabi Handout");
        var preview = catalog.GetWorkoutPreview("Stabi Handout Easy");

        Assert.Contains("Stabi Handout Easy", ids);
        Assert.Contains("Stabi Handout Epic", ids);
        Assert.Equal(4, preview.AvailableLevels.Count);
    }

    [Fact]
    public void WorkoutService_KeepsActiveWorkoutStateSeparateFromCatalog()
    {
        var service = CreateService();

        Assert.Null(service.GetActiveWorkout());

        service.SetActiveWorkout("Stabi Handout Easy", sets: 1, rounds: 1);

        var activeWorkout = service.GetActiveWorkout();
        Assert.NotNull(activeWorkout);
        Assert.Equal("Stabi Handout Easy", activeWorkout.Id);
    }

    [Fact]
    public void WorkoutPreview_CarriesContentMetadata()
    {
        var exerciseCatalog = WorkoutCatalogTestData.CreateExerciseCatalog();
        var definition = new WorkoutDefinition
        {
            Id = "stabi-tags",
            Name = "Stabi Tags",
            Description = "Tagged workout",
            Level = Level.Easy,
            Exercises = new List<string> { "Glute Bridge Easy" },
            Tags = new List<string> { "Stability", "Glute" },
            ReadMore = new List<ContentReference>
            {
                new()
                {
                    Title = "Context",
                    Url = "/academy/stability"
                }
            }
        };

        var preview = new WorkoutPreview(definition, exerciseCatalog);

        Assert.Equal(definition.Tags, preview.Tags);
        Assert.Equal("Context", Assert.Single(preview.ReadMore).Title);
    }

    private static WorkoutCatalog CreateCatalog()
    {
        return WorkoutCatalogTestData.CreateWorkoutCatalog();
    }

    private static WorkoutService CreateService()
    {
        return WorkoutCatalogTestData.CreateWorkoutService();
    }
}
