using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Services
{
    public class ExerciseCatalog : IExerciseCatalog
    {
        private readonly List<ExerciseDefinition> _definitions;
        private readonly List<ExercisePreview> _previews;

        public ExerciseCatalog(IEnumerable<ExerciseDefinition> exerciseDefinitions)
        {
            _definitions = exerciseDefinitions.ToList();
            _previews = _definitions.Select(def => new ExercisePreview(def)).ToList();
        }

        public ExerciseDefinition GetDefinition(string id, Level level)
        {
            return _definitions.Find(x => x.Id == id && x.Level == level)
                ?? throw new KeyNotFoundException($"Exercise definition '{id}' with level '{level}' was not found.");
        }

        public ExercisePreview GetExercisePreview(string id, Level level)
        {
            return _previews.Find(x => x.Id == id && x.Level == level)
                ?? throw new KeyNotFoundException($"Exercise preview '{id}' with level '{level}' was not found.");
        }

        public List<string> GetExerciseIds()
        {
            return _definitions.Select(o => o.Id).ToList();
        }
    }
}
