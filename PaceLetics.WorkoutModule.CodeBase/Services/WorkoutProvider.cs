

using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutProvider : IWorkoutProvider
    {
        private readonly List<WorkoutDefinition> _workouts;
        private readonly List<WorkoutPreview> _previews;
        private readonly IExerciseProvider _provider;
        private IWorkout? _activeWorkout;


        public WorkoutProvider(IExerciseProvider provider) 
            : this(provider, new DefinitionFactory().CreateWorkoutExamples())
        {
        }

        public WorkoutProvider(IExerciseProvider provider, IEnumerable<WorkoutDefinition> workoutDefinitions)
        {
            _provider = provider;
            _previews = new List<WorkoutPreview>();
            _workouts = workoutDefinitions.ToList();
            // Build one preview per base workout name (group variants) and collect available levels
            var baseNames = _workouts
                .Select(w => w.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct();
            foreach (var name in baseNames)
            {
                var variants = _workouts.Where(w => w.Name == name).ToList();
                var representative = variants.First();
                var levels = variants.Select(v => v.Level).Distinct();
                _previews.Add(new WorkoutPreview(representative, provider, levels));
            }

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

        public List<string> GetWorkoutIdsByName(string name)
        {
            return _workouts
                .Where(w => w.Name == name && !string.IsNullOrWhiteSpace(w.Id))
                .Select(w => w.Id!)
                .ToList();
        }

        public IWorkout GetWorkout(string id)
        {
            var def = GetDefinition(id);
            return new Workout(def, _provider);
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
            // find the definition for the id and return the preview for the group (by Name)
            var def = GetDefinition(id);
            return _previews.First(p => p.Name == def.Name);
        }

        public void SetActiveWorkout(string id, int sets, int rounds) 
        {
            var def = GetDefinition(id);
            _activeWorkout = new Workout(def, _provider, sets, rounds); 
        }

        public IWorkout? GetActiveWorkout() 
        {
            return _activeWorkout;
        }

        private WorkoutDefinition GetDefinition(string id)
        {
            return _workouts.Find(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Workout definition '{id}' was not found.");
        }
    }
}
