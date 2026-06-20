using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Services
{
    public class ExerciseCatalog : IExerciseCatalog
    {
        private readonly List<ExerciseDefinition> _definitions;

        public ExerciseCatalog(IEnumerable<ExerciseDefinition> exerciseDefinitions)
        {
            _definitions = exerciseDefinitions as List<ExerciseDefinition> ?? exerciseDefinitions.ToList();
        }

        public ExerciseDefinition GetDefinition(string id, Level level)
        {
            return _definitions.Find(x => x.Id == id && x.Level == level)
                ?? throw new KeyNotFoundException($"Exercise definition '{id}' with level '{level}' was not found.");
        }

        public ExercisePreview GetExercisePreview(string id, Level level)
        {
            var definition = GetDefinition(id, level);
            return new ExercisePreview(definition);
        }

        public List<string> GetExerciseIds()
        {
            return _definitions.Select(o => o.Id).ToList();
        }
    }
}
