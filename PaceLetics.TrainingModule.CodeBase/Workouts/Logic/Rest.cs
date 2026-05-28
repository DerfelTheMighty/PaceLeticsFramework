using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Logic
{
    public class Rest : TimedWorkoutElement, IRestInfo, IWorkoutElement
    {
        public Rest(int duration, WorkoutElements type)
            : base(type, duration)
        {
            // base kümmert sich um Timer + State + TimeRemaining
        }
    }
}
