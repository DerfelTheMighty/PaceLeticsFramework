using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.Web.ViewModels.Workouts;

public sealed class WorkoutAreaViewModel
{
    private readonly IWorkoutCatalog _workoutCatalog;
    private readonly List<WorkoutPreview> _workoutPreviews = new();

    public WorkoutAreaViewModel(IWorkoutCatalog workoutCatalog)
    {
        _workoutCatalog = workoutCatalog;
    }

    public IReadOnlyList<WorkoutPreview> WorkoutPreviews => _workoutPreviews;

    public void Initialize()
    {
        _workoutPreviews.Clear();

        foreach (var baseName in _workoutCatalog.GetBaseWorkoutNames())
        {
            var ids = _workoutCatalog.GetWorkoutIdsByName(baseName);
            if (ids.Count == 0)
                continue;

            _workoutPreviews.Add(_workoutCatalog.GetWorkoutPreview(ids.First()));
        }
    }
}
