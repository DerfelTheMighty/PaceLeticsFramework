using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Logic;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Services
{
    public class ExerciseFactory : IExerciseFactory
    {
        public Exercise Create(ExerciseDefinition definition)
        {
            return new Exercise(definition);
        }
    }
}
