

using PaceLetics.WorkoutModule.CodeBase.Enums;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IWorkoutElement
    {
        /// <summary>
        /// Defines the tyoe of workout element to simplify casting to the correct interface
        /// </summary>
        WorkoutElements Type { get; }
        /// <summary>
        /// Current activity state of the exercise
        /// </summary>
        ExerciseState State { get; }
        /// <summary>
        /// Overall exercise duration in seconds
        /// </summary>
        int Duration { get; }
        /// <summary>
        /// Remaining time in seconds
        /// </summary>
        int TimeRemaining { get; }

        int SlotDuration { get; }


        event Action FinishedEvent;
        event Action<int> ProgressChangedEvent;
        event Action<ExerciseState> StateChangedEvent;

        /// <summary>
        /// Start a new execise execution or resumes a paused execution
        /// </summary>
        public void Start();

        /// <summary>
        /// Pauses the currently running exercise
        /// </summary>
        public void Stop();

        /// <summary>
        /// Resets the exercise state
        /// </summary>
        public void Reset();

    }
}
