using PaceLetics.WorkoutModule.CodeBase.Interfaces;

namespace PaceLetics.Web.ViewModels.Workouts;

public sealed class WorkoutRoomViewModel
{
    private readonly IWorkoutService _workoutService;

    public WorkoutRoomViewModel(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
    }

    public IWorkout? Workout { get; private set; }

    public void Initialize()
    {
        Workout = _workoutService.GetActiveWorkout();
    }

    public void StopAndReset()
    {
        Workout?.Stop();
        Workout?.Reset();
    }
}
