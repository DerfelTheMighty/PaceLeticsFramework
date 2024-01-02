

using PaceLetics.WorkoutModule.CodeBase.Enums;

namespace PaceLetics.WorkoutModule.CodeBase.Models
{
    internal class Timeslot
    {
        /// <summary>
        /// 
        /// </summary>
        public ExerciseState ExerciseState{get;}
        
        /// <summary>
        /// 
        /// </summary>
        public int Duration { get; set; }

        public Timeslot(ExerciseState state, int duration) 
        {
            ExerciseState  = state;
            Duration = duration;
        }
    }
}
