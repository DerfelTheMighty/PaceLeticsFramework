using WorkoutModule.Enums;
using WorkoutModule.Logic;
using WorkoutModule.Models;

namespace WorkoutModule.Contracts
{
    public interface IExerciseProvider
    {
        ExercisePreview GetExercisePreview(string id, Level lvl);
        Exercise GetExercise(string id, Level lvl);
        List<string> GetExerciseIds();
    }
}