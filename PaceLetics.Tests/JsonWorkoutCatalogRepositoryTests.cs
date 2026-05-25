using PaceLetics.WorkoutModule.CodeBase.Repositories;

namespace PaceLetics.Tests;

public sealed class JsonWorkoutCatalogRepositoryTests
{
    [Fact]
    public void Load_ReturnsCatalogDocumentForCommittedWorkoutCatalog()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "PaceLetics.Web",
            "wwwroot",
            "data",
            "workouts",
            "catalog.de.json");

        var repository = new JsonWorkoutCatalogRepository(path);

        var catalog = repository.Load();

        Assert.NotEmpty(catalog.Exercises);
        Assert.NotEmpty(catalog.Workouts);
    }

    [Fact]
    public void Load_ReturnsCatalogDocumentForValidJson()
    {
        var path = WriteTempCatalog("""
        {
          "schemaVersion": 1,
          "exercises": [
            {
              "id": "glute-bridge",
              "name": "Glute Bridge",
              "description": "Test exercise",
              "execution": ["Start"],
              "duration": 30,
              "imageFile": "glute_bridge_base.png",
              "level": "Easy",
              "switchLeftRight": false,
              "switchTime": 0
            }
          ],
          "workouts": [
            {
              "id": "stabi-easy",
              "name": "Stabi",
              "description": "Test workout",
              "level": "Easy",
              "preparationTime": 10,
              "restTime": 10,
              "switchTime": 5,
              "exercises": ["glute-bridge"]
            }
          ]
        }
        """);

        var repository = new JsonWorkoutCatalogRepository(path);

        var catalog = repository.Load();

        Assert.Single(catalog.Exercises);
        Assert.Single(catalog.Workouts);
    }

    [Fact]
    public void Load_ThrowsValidationExceptionForMissingExerciseReference()
    {
        var path = WriteTempCatalog("""
        {
          "schemaVersion": 1,
          "exercises": [
            {
              "id": "glute-bridge",
              "name": "Glute Bridge",
              "description": "Test exercise",
              "execution": ["Start"],
              "duration": 30,
              "imageFile": "glute_bridge_base.png",
              "level": "Easy",
              "switchLeftRight": false,
              "switchTime": 0
            }
          ],
          "workouts": [
            {
              "id": "stabi-easy",
              "name": "Stabi",
              "description": "Test workout",
              "level": "Easy",
              "preparationTime": 10,
              "restTime": 10,
              "switchTime": 5,
              "exercises": ["missing-exercise"]
            }
          ]
        }
        """);

        var repository = new JsonWorkoutCatalogRepository(path);

        var ex = Assert.Throws<WorkoutCatalogValidationException>(() => repository.Load());
        Assert.Contains(ex.Errors, error => error.Contains("missing-exercise"));
    }

    private static string WriteTempCatalog(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PaceLeticsFramework.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate PaceLeticsFramework.sln.");
    }
}
