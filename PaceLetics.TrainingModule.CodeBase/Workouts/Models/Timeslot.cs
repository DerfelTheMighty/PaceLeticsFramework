

using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Models
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
