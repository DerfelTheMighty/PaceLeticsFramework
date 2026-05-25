using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutProvider : IWorkoutProvider
    {
        private readonly IWorkoutCatalog _workoutCatalog;
        private readonly IWorkoutService _workoutService;

        public WorkoutProvider(IExerciseProvider provider)
            : this(new WorkoutCatalog(provider), new WorkoutFactory(provider))
        {
        }

        public WorkoutProvider(IExerciseProvider provider, IEnumerable<WorkoutDefinition> workoutDefinitions)
            : this(new WorkoutCatalog(provider, workoutDefinitions), new WorkoutFactory(provider))
        {
        }

        public WorkoutProvider(
            IExerciseProvider provider,
            IWorkoutFactory workoutFactory,
            IEnumerable<WorkoutDefinition> workoutDefinitions)
            : this(new WorkoutCatalog(provider, workoutDefinitions), workoutFactory)
        {
        }

        public WorkoutProvider(IWorkoutCatalog workoutCatalog, IWorkoutService workoutService)
        {
            _workoutCatalog = workoutCatalog;
            _workoutService = workoutService;
        }

        private WorkoutProvider(IWorkoutCatalog workoutCatalog, IWorkoutFactory workoutFactory)
            : this(workoutCatalog, new WorkoutService(workoutCatalog, workoutFactory))
        {
        }

        public List<string> GetBaseWorkoutNames()
        {
            return _workoutCatalog.GetBaseWorkoutNames();
        }

        public List<string> GetWorkoutIdsByName(string name)
        {
            return _workoutCatalog.GetWorkoutIdsByName(name);
        }

        public IWorkout GetWorkout(string id)
        {
            return _workoutService.GetWorkout(id);
        }

        public List<string> GetWorkoutIds()
        {
            return _workoutCatalog.GetWorkoutIds();
        }

        public WorkoutPreview GetWorkoutPreview(string id)
        {
            return _workoutCatalog.GetWorkoutPreview(id);
        }

        public void SetActiveWorkout(string id, int sets, int rounds)
        {
            _workoutService.SetActiveWorkout(id, sets, rounds);
        }

        public IWorkout? GetActiveWorkout()
        {
            return _workoutService.GetActiveWorkout();
        }
    }
}
