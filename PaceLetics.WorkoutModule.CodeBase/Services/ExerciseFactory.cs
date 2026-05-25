using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class ExerciseFactory : IExerciseFactory
    {
        public Exercise Create(ExerciseDefinition definition)
        {
            return new Exercise(definition);
        }
    }
}
