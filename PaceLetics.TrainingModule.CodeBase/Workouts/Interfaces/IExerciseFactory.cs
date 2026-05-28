using PaceLetics.TrainingModule.CodeBase.Workouts.Logic;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces
{
    public interface IExerciseFactory
    {
        Exercise Create(ExerciseDefinition definition);
    }
}
