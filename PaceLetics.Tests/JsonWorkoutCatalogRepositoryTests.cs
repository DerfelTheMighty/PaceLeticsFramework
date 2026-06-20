using PaceLetics.TrainingModule.CodeBase.Workouts.Repositories;

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
              "switchTime": 0,
              "tags": ["Glute", " tabata ", "glute"],
              "source": "Official PaceLetics",
              "ownerUserId": "trainer-1",
              "readMore": [
                {
                  "title": "Reference",
                  "url": "https://example.com/reference",
                  "description": "Background",
                  "sourceType": "study"
                }
              ]
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
              "exercises": ["glute-bridge"],
              "tags": ["Stability", "stability"],
              "readMore": [
                {
                  "title": "Workout context",
                  "url": "/academy/stability"
                }
              ]
            }
          ]
        }
        """);

        var repository = new JsonWorkoutCatalogRepository(path);

        var catalog = repository.Load();

        Assert.Single(catalog.Exercises);
        Assert.Single(catalog.Workouts);
        Assert.Equal(new[] { "Glute", "tabata" }, catalog.Exercises[0].Tags);
        Assert.Equal("Official PaceLetics", catalog.Exercises[0].Source);
        Assert.Equal("trainer-1", catalog.Exercises[0].OwnerUserId);
        var exerciseReference = Assert.Single(catalog.Exercises[0].ReadMore);
        Assert.Equal("Reference", exerciseReference.Title);
        Assert.Equal("https://example.com/reference", exerciseReference.Url);
        Assert.Equal(new[] { "Stability" }, catalog.Workouts[0].Tags);
        Assert.Equal("/academy/stability", Assert.Single(catalog.Workouts[0].ReadMore).Url);
    }

    [Fact]
    public void Load_ThrowsValidationExceptionForEmptyReadMoreReference()
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
              "switchTime": 0,
              "readMore": [
                {}
              ]
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

        var ex = Assert.Throws<WorkoutCatalogValidationException>(() => repository.Load());
        Assert.Contains(ex.Errors, error => error.Contains("empty readMore reference"));
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

    private static string FindRepositoryRoot(
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        var sourceDirectory = string.IsNullOrWhiteSpace(sourceFilePath)
            ? null
            : new DirectoryInfo(Path.GetDirectoryName(sourceFilePath)!);
        var directory = FindRepositoryRootFrom(sourceDirectory)
            ?? FindRepositoryRootFrom(new DirectoryInfo(Directory.GetCurrentDirectory()))
            ?? FindRepositoryRootFrom(new DirectoryInfo(AppContext.BaseDirectory));

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate PaceLeticsFramework.sln.");
    }

    private static DirectoryInfo? FindRepositoryRootFrom(DirectoryInfo? directory)
    {
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "PaceLeticsFramework.sln")))
        {
            directory = directory.Parent;
        }

        return directory;
    }
}
