using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.TrainingModule.CodeBase.Workouts.Services;

namespace PaceLetics.Tests;

public sealed class ExerciseCatalogFactoryTests
{
    [Fact]
    public void ExerciseCatalog_ReturnsDefinitionAndPreviewForKnownExercise()
    {
        var catalog = WorkoutCatalogTestData.CreateExerciseCatalog();

        var definition = catalog.GetDefinition("Glute Bridge Easy", Level.Easy);
        var preview = catalog.GetExercisePreview("Glute Bridge Easy", Level.Easy);

        Assert.Equal(definition.Id, preview.Id);
        Assert.Equal(definition.Name, preview.Name);
        Assert.Equal(definition.Level, preview.Level);
    }

    [Fact]
    public void ExerciseFactory_CreatesRuntimeExerciseFromDefinition()
    {
        var catalog = WorkoutCatalogTestData.CreateExerciseCatalog();
        var factory = new ExerciseFactory();
        var definition = catalog.GetDefinition("Glute Bridge Easy", Level.Easy);

        var exercise = factory.Create(definition);

        Assert.Equal(definition.Id, exercise.Id);
        Assert.Equal(definition.Duration, exercise.Duration);
        Assert.Equal(definition.Level, exercise.Level);
    }

    [Fact]
    public void ExercisePreview_CarriesContentMetadata()
    {
        var definition = new ExerciseDefinition
        {
            Id = "jumping-lunges",
            Name = "Jumping Lunges",
            Description = "Explosive leg exercise",
            Level = Level.Moderate,
            Tags = new List<string> { "Tabata", "Glute" },
            ReadMore = new List<ContentReference>
            {
                new()
                {
                    Title = "Background",
                    Url = "https://example.com/background"
                }
            }
        };

        var preview = new ExercisePreview(definition);

        Assert.Equal(definition.Tags, preview.Tags);
        Assert.Equal("Background", Assert.Single(preview.ReadMore).Title);
    }
}
