using WorkoutModule.Logic;

namespace WorkoutModule.Contracts
{
    public interface IExerciseProvider
    {
        Exercise GetExercise(string id);
        List<string> GetExerciseIds();
    }
}