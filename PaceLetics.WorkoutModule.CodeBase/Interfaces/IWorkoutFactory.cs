using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IWorkoutFactory
    {
        IWorkout Create(WorkoutDefinition definition);
        IWorkout Create(WorkoutDefinition definition, WorkoutBuildOptions options);
    }
}
