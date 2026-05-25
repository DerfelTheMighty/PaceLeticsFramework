using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IExerciseFactory
    {
        Exercise Create(ExerciseDefinition definition);
    }
}
