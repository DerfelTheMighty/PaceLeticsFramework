using WorkoutModule.Enums;

namespace WorkoutModule.Contracts
{
    public interface IExerciseInfo
    {

        /// <summary>
        /// public name of the exercise
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Unique Id the exercise
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Short description of the exercise
        /// </summary>
        string Description { get; }

        /// <summary>
        /// bullet point describing the execution
        /// </summary>
        List<string> Execution { get; }

        /// <summary>
        /// Imagefilename to illustrate the exercise
        /// </summary>
        string ImageFilename { get; }

        /// <summary>
        /// Difficulty level of the excerise
        /// </summary>
        Level Level { get; }

        /// <summary>
        /// Overall exercise duration in seconds
        /// </summary>
        int Duration { get; }

        /// <summary>
        /// Flag is true, if the exercise has to switch between left and right
        /// </summary>
        bool SwitchLeftRight { get; }

        /// <summary>
        /// Remaining time in seconds
        /// </summary>
        int TimeRemaining { get; }

        /// <summary>
        /// Current activity state of the exercise
        /// </summary>
        ExerciseState State { get; }

        /// <summary>
        /// Event is executed each second
        /// </summary>
        Action<int> ProgressChangedEvent { get; set; }

        // Event is execute whenever the exercise state changes
        Action<ExerciseState> StateChangedEvent { get; set; }

    }

}
