using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutCatalog : IWorkoutCatalog
    {
        private readonly List<WorkoutDefinition> _workouts;
        private readonly List<WorkoutPreview> _previews;

        public WorkoutCatalog(IExerciseCatalog exerciseCatalog)
            : this(exerciseCatalog, new DefinitionFactory().CreateWorkoutExamples())
        {
        }

        public WorkoutCatalog(
            IExerciseCatalog exerciseCatalog,
            IEnumerable<WorkoutDefinition> workoutDefinitions)
        {
            _workouts = workoutDefinitions.ToList();
            _previews = BuildPreviews(exerciseCatalog);
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
            return _previews.First(p => p.Name == def.Name);
        }

        private List<WorkoutPreview> BuildPreviews(IExerciseCatalog exerciseCatalog)
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
                previews.Add(new WorkoutPreview(representative, exerciseCatalog, levels));
            }

            return previews;
        }
    }
}
