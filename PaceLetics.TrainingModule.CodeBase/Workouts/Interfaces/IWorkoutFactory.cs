using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces
{
    public interface IWorkoutFactory
    {
        IWorkout Create(WorkoutDefinition definition);
        IWorkout Create(WorkoutDefinition definition, WorkoutBuildOptions options);
    }
}
