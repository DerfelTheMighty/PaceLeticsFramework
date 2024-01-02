

using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IExerciseProvider
    {
        ExercisePreview GetExercisePreview(string id, Level lvl);
        Exercise GetExercise(string id, Level lvl);
        List<string> GetExerciseIds();
    }
}