

using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutProvider : IWorkoutProvider
    {
        private List<WorkoutDefinition> _workouts;
        private List<WorkoutPreview> _previews;
        private IExerciseProvider _provider;
        private IWorkout _activeWorkout;


        public WorkoutProvider(IExerciseProvider provider) 
        {
            _provider = provider;
            _previews = new List<WorkoutPreview>();
            DefinitionFactory factory = new DefinitionFactory();
            _workouts = factory.CreateWorkoutExamples();
            // Build one preview per base workout name (group variants) and collect available levels
            var baseNames = _workouts.Select(w => w.Name).Distinct();
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
            return _workouts.Select(w => w.Name).Distinct().ToList();
        }

        public List<string> GetWorkoutIdsByName(string name)
        {
            return _workouts.Where(w => w.Name == name).Select(w => w.Id).ToList();
        }

        public IWorkout GetWorkout(string id)
        {
            var def = _workouts.Find(x => x.Id == id);
            return new Workout(def, _provider);
        }

        public List<string> GetWorkoutIds()
        {
            return _workouts.Select(o => o.Id).ToList();
        }

        public WorkoutPreview GetWorkoutPreview(string id)
        {
            // find the definition for the id and return the preview for the group (by Name)
            var def = _workouts.Find(x => x.Id == id);
            if (def != null)
                return _previews.Find(p => p.Name == def.Name);
            return _previews.Find(x => x.Id == id);
        }

        public void SetActiveWorkout(string id, int sets, int rounds) 
        {
            var def = _workouts.Find(x => x.Id == id);
            if(def !=null)
                _activeWorkout = new Workout(def, _provider, sets, rounds); 
        }

        public IWorkout GetActiveWorkout() 
        {
            return _activeWorkout;
        }
    }
}
