using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutFactory : IWorkoutFactory
    {
        private readonly IExerciseProvider _exerciseProvider;

        public WorkoutFactory(IExerciseProvider exerciseProvider)
        {
            _exerciseProvider = exerciseProvider;
        }

        public IWorkout Create(WorkoutDefinition definition)
        {
            return Create(definition, new WorkoutBuildOptions());
        }

        public IWorkout Create(WorkoutDefinition definition, WorkoutBuildOptions options)
        {
            return new Workout(definition, _exerciseProvider, options);
        }
    }
}
