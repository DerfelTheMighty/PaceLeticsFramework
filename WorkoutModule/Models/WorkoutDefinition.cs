
using WorkoutModule.Enums;

namespace WorkoutModule.Models
{

    /// <summary>
    /// Defines a complete workout consisting of several exercises
    /// </summary>
    public class WorkoutDefinition
    {
        // Name of the workout
        public string? Name { get; set; }

        /// <summary>
        /// Unique identifier of the workout
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Short description of the workout
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Difficulty level of the exercise
        /// </summary>
        public Level Level { get; set; }

        /// <summary>
        /// Time before the first exercise starts
        /// </summary>
        public int PreparationTime { get; set; }
        /// <summary>
        /// Rest time between two exercises
        /// </summary>
        public int RestTime { get; set; }
        /// <summary>
        /// Switch time for left/right switch exercises
        /// </summary>
        public int SwitchTime { get; set; } 
        /// <summary>
        /// List of exercises in the workout
        /// </summary>
        public List<string>? Exercises { get; set; }
        

    }

}
