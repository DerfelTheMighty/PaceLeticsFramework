using PaceLetics.WorkoutModule.CodeBase.Models;
using PaceLetics.WorkoutModule.CodeBase.Repositories;
using PaceLetics.WorkoutModule.CodeBase.Services;

namespace PaceLetics.Tests;

internal static class WorkoutCatalogTestData
{
    public static WorkoutCatalogDocument LoadDocument()
    {
        return new JsonWorkoutCatalogRepository(GetCatalogPath()).Load();
    }

    public static ExerciseCatalog CreateExerciseCatalog()
    {
        return new ExerciseCatalog(LoadDocument().Exercises);
    }

    public static WorkoutCatalog CreateWorkoutCatalog()
    {
        var document = LoadDocument();
        return new WorkoutCatalog(new ExerciseCatalog(document.Exercises), document.Workouts);
    }

    public static WorkoutService CreateWorkoutService()
    {
        var document = LoadDocument();
        var exerciseCatalog = new ExerciseCatalog(document.Exercises);
        var workoutCatalog = new WorkoutCatalog(exerciseCatalog, document.Workouts);
        var workoutFactory = new WorkoutFactory(exerciseCatalog, new ExerciseFactory());
        return new WorkoutService(workoutCatalog, workoutFactory);
    }

    private static string GetCatalogPath()
    {
        return Path.Combine(
            FindRepositoryRoot(),
            "PaceLetics.Web",
            "wwwroot",
            "data",
            "workouts",
            "catalog.de.json");
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
