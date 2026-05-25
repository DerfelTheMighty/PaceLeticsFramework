using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Services;

namespace PaceLetics.Tests;

public class WorkoutServiceTests
{
    [Fact]
    public void GetActiveWorkout_ReturnsNullBeforeSelection()
    {
        var service = CreateService();

        Assert.Null(service.GetActiveWorkout());
    }

    [Fact]
    public void GetWorkout_ThrowsForUnknownId()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() => service.GetWorkout("missing-workout"));
    }

    [Fact]
    public void ExerciseProvider_ThrowsForUnknownExercise()
    {
        var provider = new ExerciseProvider();

        Assert.Throws<KeyNotFoundException>(() => provider.GetExercise("missing-exercise", Level.Easy));
    }

    [Fact]
    public void SetActiveWorkout_UsesSetsAndRoundsWhenBuildingWorkout()
    {
        var service = CreateService();

        service.SetActiveWorkout("Stabi Handout Easy", sets: 2, rounds: 2);

        var workout = service.GetActiveWorkout();
        Assert.NotNull(workout);
        Assert.Equal(64, workout.Elements.Count);
        Assert.Equal(32, workout.Exercises.Count);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void SetActiveWorkout_ThrowsForInvalidBuildOptions(int sets, int rounds)
    {
        var service = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.SetActiveWorkout("Stabi Handout Easy", sets, rounds));
    }

    private static IWorkoutService CreateService()
    {
        var exerciseProvider = new ExerciseProvider();
        return new WorkoutService(
            new WorkoutCatalog(exerciseProvider),
            new WorkoutFactory(exerciseProvider));
    }
}
