using PaceLetics.WorkoutModule.CodeBase.Services;

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

    private static WorkoutCatalog CreateCatalog()
    {
        return new WorkoutCatalog(new ExerciseProvider());
    }

    private static WorkoutService CreateService()
    {
        var exerciseProvider = new ExerciseProvider();
        var catalog = new WorkoutCatalog(exerciseProvider);
        var factory = new WorkoutFactory(exerciseProvider);
        return new WorkoutService(catalog, factory);
    }
}
