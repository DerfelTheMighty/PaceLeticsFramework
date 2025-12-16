

using PaceLetics.WorkoutModule.CodeBase.Enums;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IWorkout
    {
        /// <summary>
        /// Index of the currently active exercise
        /// </summary>
        int CurrentElement { get; }
        /// <summary>
        /// The list element containing the currently active exercise
        /// </summary>
        int CurrentExercise { get; }
        /// <summary>
        /// Short description of the workout
        /// </summary>
        string Description { get; }

        // Preview image location
        string Imagefile { get; }

        /// <summary>
        /// Public name 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
		IReadOnlyCollection<IWorkoutElement> Elements { get; }

		/// <summary>
		/// List containing all exercises in the workout
		/// </summary>
		List<IExerciseInfo> Exercises { get; }
        /// <summary>
        /// Unique id of the workout
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Difficulty level of the exercise
        /// </summary>
        Level Level { get; }
        /// <summary>
        /// Time before the first exercise starts
        /// </summary>
        int PreparationTime { get; }
        /// <summary>
        /// Rest time between two exercises
        /// </summary>
        int RestTime { get; }
        /// <summary>
        /// Workout state 
        /// </summary>
        WorkoutState State { get; }
        /// <summary>
        /// Event is fired when all workout elements are done
        /// </summary>
        event Action WorkoutFinishedEvent;
        event Action<IWorkoutElement> ElementFinishedEvent;
        event Action<IWorkoutElement> ElementStartEvent;
        event Action WorkoutStartEvent;

        /// <summary>
        /// Resets the currently paused workout and the currently paused workout element
        /// </summary>
        void Reset();
        /// <summary>
        /// Starts a new workout or resumes a paused workout
        /// </summary>
        void Start();
        /// <summary>
        /// Pauses the workout by halting the currently active workout element
        /// </summary>
        void Stop();
    }
}