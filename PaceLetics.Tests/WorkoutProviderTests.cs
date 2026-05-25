using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Services;

namespace PaceLetics.Tests;

public class WorkoutProviderTests
{
    [Fact]
    public void GetActiveWorkout_ReturnsNullBeforeSelection()
    {
        var provider = CreateProvider();

        Assert.Null(provider.GetActiveWorkout());
    }

    [Fact]
    public void GetWorkout_ThrowsForUnknownId()
    {
        var provider = CreateProvider();

        Assert.Throws<KeyNotFoundException>(() => provider.GetWorkout("missing-workout"));
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
        var provider = CreateProvider();

        provider.SetActiveWorkout("Stabi Handout Easy", sets: 2, rounds: 2);

        var workout = provider.GetActiveWorkout();
        Assert.NotNull(workout);
        Assert.Equal(64, workout.Elements.Count);
        Assert.Equal(32, workout.Exercises.Count);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void SetActiveWorkout_ThrowsForInvalidBuildOptions(int sets, int rounds)
    {
        var provider = CreateProvider();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            provider.SetActiveWorkout("Stabi Handout Easy", sets, rounds));
    }

    private static WorkoutProvider CreateProvider()
    {
        return new WorkoutProvider(new ExerciseProvider());
    }
}
