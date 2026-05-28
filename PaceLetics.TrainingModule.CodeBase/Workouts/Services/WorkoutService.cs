using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutCatalog _workoutCatalog;
        private readonly IWorkoutFactory _workoutFactory;
        private IWorkout? _activeWorkout;

        public WorkoutService(IWorkoutCatalog workoutCatalog, IWorkoutFactory workoutFactory)
        {
            _workoutCatalog = workoutCatalog;
            _workoutFactory = workoutFactory;
        }

        public IWorkout GetWorkout(string id)
        {
            return _workoutFactory.Create(_workoutCatalog.GetDefinition(id));
        }

        public void SetActiveWorkout(string id, int sets, int rounds)
        {
            var definition = _workoutCatalog.GetDefinition(id);
            _activeWorkout = _workoutFactory.Create(definition, new WorkoutBuildOptions(sets, rounds));
        }

        public IWorkout? GetActiveWorkout()
        {
            return _activeWorkout;
        }
    }
}
