using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Services
{
    public class WorkoutCatalog : IWorkoutCatalog
    {
        private readonly List<WorkoutDefinition> _workouts;
        private readonly IExerciseCatalog _exerciseCatalog;

        public WorkoutCatalog(
            IExerciseCatalog exerciseCatalog,
            IEnumerable<WorkoutDefinition> workoutDefinitions)
        {
            _exerciseCatalog = exerciseCatalog;
            _workouts = workoutDefinitions as List<WorkoutDefinition> ?? workoutDefinitions.ToList();
        }

        public List<string> GetBaseWorkoutNames()
        {
            return _workouts
                .Select(w => w.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct()
                .ToList();
        }

        public WorkoutDefinition GetDefinition(string id)
        {
            return _workouts.Find(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Workout definition '{id}' was not found.");
        }

        public List<string> GetWorkoutIdsByName(string name)
        {
            return _workouts
                .Where(w => w.Name == name && !string.IsNullOrWhiteSpace(w.Id))
                .Select(w => w.Id!)
                .ToList();
        }

        public List<string> GetWorkoutIds()
        {
            return _workouts
                .Select(o => o.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id!)
                .ToList();
        }

        public WorkoutPreview GetWorkoutPreview(string id)
        {
            var def = GetDefinition(id);
            return BuildPreviews().First(p => p.Name == def.Name);
        }

        private List<WorkoutPreview> BuildPreviews()
        {
            var previews = new List<WorkoutPreview>();
            var baseNames = _workouts
                .Select(w => w.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct();

            foreach (var name in baseNames)
            {
                var variants = _workouts.Where(w => w.Name == name).ToList();
                var representative = variants.First();
                var levels = variants.Select(v => v.Level).Distinct();
                previews.Add(new WorkoutPreview(representative, _exerciseCatalog, levels));
            }

            return previews;
        }
    }
}
