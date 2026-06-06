using PaceLetics.Web.ViewModels.Workouts;
using PaceLetics.TrainingModule.CodeBase.Workouts.Services;

namespace PaceLetics.Tests;

public sealed class WorkoutViewModelTests
{
    [Fact]
    public void WorkoutAreaViewModel_LoadsWorkoutPreviewsFromCatalog()
    {
        var viewModel = new WorkoutAreaViewModel(WorkoutCatalogTestData.CreateWorkoutCatalog());

        viewModel.Initialize();

        Assert.NotEmpty(viewModel.WorkoutPreviews);
        Assert.Contains(viewModel.WorkoutPreviews, preview => preview.Name == "Stabi Handout");
    }

    [Fact]
    public void SelectDifficultyViewModel_LoadsVariantsAndStartsWorkout()
    {
        var document = WorkoutCatalogTestData.LoadDocument();
        var exerciseCatalog = new ExerciseCatalog(document.Exercises);
        var workoutCatalog = new WorkoutCatalog(exerciseCatalog, document.Workouts);
        var workoutService = new WorkoutService(workoutCatalog, new WorkoutFactory(exerciseCatalog, new ExerciseFactory()));
        var viewModel = new SelectDifficultyViewModel(workoutCatalog, workoutService);

        viewModel.Initialize("Stabi Handout");
        viewModel.Sets = 2;
        viewModel.Rounds = 2;
        viewModel.StartWorkout("Stabi Handout Easy");

        Assert.Contains("Stabi Handout Easy", viewModel.Variants);
        Assert.NotEqual("images/exercises/no_image.png", viewModel.PreviewImage);
        Assert.NotNull(workoutService.GetActiveWorkout());
        Assert.Equal(32, workoutService.GetActiveWorkout()!.Exercises.Count);
    }

    [Fact]
    public void WorkoutRoomViewModel_LoadsAndResetsActiveWorkout()
    {
        var workoutService = WorkoutCatalogTestData.CreateWorkoutService();
        workoutService.SetActiveWorkout("Stabi Handout Easy", sets: 1, rounds: 1);
        var viewModel = new WorkoutRoomViewModel(workoutService);

        viewModel.Initialize();
        viewModel.StopAndReset();

        Assert.NotNull(viewModel.Workout);
    }
}
