using WorkoutModule.Enums;


namespace WorkoutModule.Models
{
    /// <summary>
    /// Serializable Definition of an exercise
    /// </summary>
    public class ExerciseDefinition
    {
        // Public name of the exercise
        public string? Name { get; set; }
        /// <summary>
        /// Unique id of the exersice
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Short description of the exercise goals
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// bullet point describing the execution
        /// </summary>
        public List<string> Execution { get; set; }

        /// <summary>
        /// Duration in seconds
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// Path to an image illustrating the exercise
        /// </summary>
        public string? ImageFile { get; set; }
        /// <summary>
        /// Difficulty level of the exercise
        /// </summary>
        public Level Level { get; set; }
        /// <summary>
        /// Flag is true, if the exercise has to switch between left and right
        /// </summary>
        public bool SwitchLeftRight { get; set; }

        /// <summary>
        /// Time to switch the orientation in Left/Right exercises
        /// </summary>
        public int SwitchTime { get; set; }
    }

}
