using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;

namespace PaceLetics.WorkoutModule.CodeBase.Logic
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
