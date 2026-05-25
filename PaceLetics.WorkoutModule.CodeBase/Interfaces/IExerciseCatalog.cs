using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IExerciseCatalog
    {
        ExerciseDefinition GetDefinition(string id, Level level);
        ExercisePreview GetExercisePreview(string id, Level level);
        List<string> GetExerciseIds();
    }
}
