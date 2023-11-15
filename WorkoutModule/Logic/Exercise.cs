using WorkoutModule.Enums;
using System.Timers;
using WorkoutModule.Models;
using WorkoutModule.Contracts;

namespace WorkoutModule.Logic
{
    public class Exercise : IExerciseInfo , IWorkoutElement
    {
        private System.Timers.Timer _timer;
        private ExerciseState _state;
        private int _timeRemaining;
        private Timeslot[] _timeslot;
        private int _currentTimeSlot;

        #region Public properties

        /// <summary>
        /// Defines the tyoe of workout element to simplify casting to the correct interface
        /// </summary>
        public WorkoutElement Type { get; }

        /// <summary>
        /// Name of the exercise
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Short description of the exercise
        /// </summary>
        public string Description { get;  }

        /// <summary>
        /// bullet point describing the execution
        /// </summary>
        public List<string> Execution { get;  }

        /// <summary>
        /// Imagefilename to illustrate the exercise
        /// </summary>
        public string ImageFilename { get; }

        /// <summary>
        /// Difficulty level of the excerise
        /// </summary>
        public Level Level { get; }

        /// <summary>
        /// Overall exercise duration in seconds
        /// </summary>
        public int Duration { get;}

        /// <summary>
        /// Flag is true, if the exercise has to switch between left and right
        /// </summary>
        public bool SwitchLeftRight { get;  }

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

        #endregion

        #region Events

        /// <summary>
        ///  Event indicates the exercise completion
        /// </summary>
        public Action? FinishedEvent { get; set; }

        /// <summary>
        /// Event is executed each second
        /// </summary>
        public Action<int>? ProgressChangedEvent { get; set; }

        // Event is execute whenever the exercise state changes
        public Action<ExerciseState>?  StateChangedEvent { get; set; }

        #endregion

        public Exercise(ExerciseDefinition definition)
        {
            Id = definition.Id ?? string.Empty;
            Description = definition.Description ?? string.Empty;
            Execution = definition.Execution ?? new List<string>() { string.Empty};
            ImageFilename = definition.ImageFile ?? string.Empty;
            Level = definition.Level;
            SwitchLeftRight = definition.SwitchLeftRight;
            Duration = definition.Duration;
            Type = WorkoutElement.Exercise;
            if (SwitchLeftRight)
            {
                _timeslot = new Timeslot[]
                {
                    new Timeslot(ExerciseState.Running, definition.Duration),
                    new Timeslot(ExerciseState.Switch, definition.SwitchTime),
                    new Timeslot(ExerciseState.Running, definition.Duration),
                };
            }
            else 
            {
                _timeslot = new Timeslot[]
                {
                    new Timeslot(ExerciseState.Running, definition.Duration)
                };
            }
            _currentTimeSlot = 0;

            State = ExerciseState.Stop;
            _timer = new System.Timers.Timer(1000); 
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            TimeRemaining = _timeslot[0].Duration;
        }



        #region public methods


        /// <summary>
        /// Start a new execise execution or resumes a paused execution
        /// </summary>
        public void Start()
        {
            switch(State)
            {
                case ExerciseState.Stop:
                    State = ExerciseState.Running;
                    ProcessTimeSlot();
                    break;
                case ExerciseState.Pause:
                    State = ExerciseState.Running;
                    _timer.Start();
                    break;
            }
        }

        /// <summary>
        /// Pauses the currently running exercise
        /// </summary>
        public void Stop()
        {
            if (State == ExerciseState.Running || State == ExerciseState.Switch)
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
            if (_state == ExerciseState.Pause)
            {
                State = ExerciseState.Stop;
                TimeRemaining = _timeslot[0].Duration;
                _currentTimeSlot = 0;
            }
        }

        #endregion

        #region Private methods
        private void ProcessTimeSlot() 
        {
            if (_currentTimeSlot < _timeslot.Length)
            {
                State = _timeslot[_currentTimeSlot].ExerciseState;
                TimeRemaining = _timeslot[_currentTimeSlot].Duration;
                _timer.Start();
            }
            else
                FinishExercise();
        }
        
        private void FinishExercise()
        {
            State = ExerciseState.Stop;
            FinishedEvent?.Invoke();
            TimeRemaining = _timeslot[0].Duration;
            _currentTimeSlot = 0;
        }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
            }
            else if (TimeRemaining == 0) 
            {
                _timer.Stop();
                _currentTimeSlot++;
                ProcessTimeSlot();
            }
        }


        #endregion
    }
}
