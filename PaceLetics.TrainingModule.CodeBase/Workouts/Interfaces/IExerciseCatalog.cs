using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces
{
    public interface IExerciseCatalog
    {
        ExerciseDefinition GetDefinition(string id, Level level);
        ExercisePreview GetExercisePreview(string id, Level level);
        List<string> GetExerciseIds();
    }
}
