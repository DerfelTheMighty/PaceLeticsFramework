using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Services;

namespace PaceLetics.Tests;

public sealed class ExerciseCatalogFactoryTests
{
    [Fact]
    public void ExerciseCatalog_ReturnsDefinitionAndPreviewForKnownExercise()
    {
        var catalog = new ExerciseCatalog();

        var definition = catalog.GetDefinition("Glute Bridge Easy", Level.Easy);
        var preview = catalog.GetExercisePreview("Glute Bridge Easy", Level.Easy);

        Assert.Equal(definition.Id, preview.Id);
        Assert.Equal(definition.Name, preview.Name);
        Assert.Equal(definition.Level, preview.Level);
    }

    [Fact]
    public void ExerciseFactory_CreatesRuntimeExerciseFromDefinition()
    {
        var catalog = new ExerciseCatalog();
        var factory = new ExerciseFactory();
        var definition = catalog.GetDefinition("Glute Bridge Easy", Level.Easy);

        var exercise = factory.Create(definition);

        Assert.Equal(definition.Id, exercise.Id);
        Assert.Equal(definition.Duration, exercise.Duration);
        Assert.Equal(definition.Level, exercise.Level);
    }
}
