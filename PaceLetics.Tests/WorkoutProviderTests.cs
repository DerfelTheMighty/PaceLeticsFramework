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

    private static WorkoutProvider CreateProvider()
    {
        return new WorkoutProvider(new ExerciseProvider());
    }
}
