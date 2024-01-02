using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using System.Timers;



namespace PaceLetics.WorkoutModule.CodeBase.Logic
{
    public class Rest : IRestInfo, IWorkoutElement
    {
        int _timeRemaining;
        ExerciseState _state;
        private System.Timers.Timer _timer;

        /// <summary>
        /// Defines the tyoe of workout element to simplify casting to the correct interface
        /// </summary>
        public WorkoutElements Type { get; }

        /// <summary>
        /// Current activity state of the exercise
        /// </summary>
        public ExerciseState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    StateChangedEvent?.Invoke(_state);
                }
            }
        }

        /// <summary>
        /// Overall exercise duration in seconds
        /// </summary>
        public int Duration { get; }

        public int SlotDuration { get;  }

        /// <summary>
        /// Remaining time in seconds
        /// </summary>
        public int TimeRemaining
        {
            get => _timeRemaining;
            private set
            {
                if (_timeRemaining != value)
                {
                    _timeRemaining = value;
                    ProgressChangedEvent?.Invoke(_timeRemaining);
                }
            }
        }


        /// <summary>
        ///  Event indicates the exercise completion
        /// </summary>
        public Action FinishedEvent { get; set; }

        /// <summary>
        /// Event is executed each second
        /// </summary>
        public Action<int> ProgressChangedEvent { get; set; }

        // Event is executed whenever the exercise state changes
        public Action<ExerciseState> StateChangedEvent { get; set; }

        public Rest(int duration, WorkoutElements type) 
        {
            Duration = duration;
            Type = type;
            _timer = new System.Timers.Timer(1000); // Set up the timer to tick every second
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            State = ExerciseState.Stop;
            TimeRemaining = Duration;
            SlotDuration = Duration;
        }


        /// <summary>
        /// Start a new execise execution
        /// </summary>
        public void Start() 
        {

            if (State == ExerciseState.Stop)
            {
                State = ExerciseState.Running;
            }
            else if(State == ExerciseState.Pause)
            {
                State = ExerciseState.Running;
            }

            _timer.Start();
        }

        /// <summary>
        /// Stops the currently running exercise
        /// </summary>
        public void Stop() 
        {
            if (State == ExerciseState.Running) 
            {
                _timer.Stop();
                State = ExerciseState.Pause;
            }
        }


        /// <summary>
        /// Resets the exercise state
        /// </summary>
        public void Reset() 
        {
            if (State == ExerciseState.Pause ) 
            {
                State = ExerciseState.Stop;
                TimeRemaining = 10;

            }
                
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
            }
            else
            {
                _timer.Stop();
                State = ExerciseState.Stop;
                FinishedEvent?.Invoke();
                TimeRemaining = 10;
            }
        }


    }
}
