using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using System.Timers;

namespace PaceLetics.WorkoutModule.CodeBase.Logic
{
    /// <summary>
    /// Shared timer/state handling for workout elements that count down in 1s ticks.
    /// </summary>
    public abstract class TimedWorkoutElement : IWorkoutElement, IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private bool _disposed;

        private ExerciseState _state;
        private int _timeRemaining;
        private int _slotDuration;

        public WorkoutElements Type { get; }
        public int Duration { get; }

        public int SlotDuration
        {
            get => _slotDuration;
            protected set => _slotDuration = value;
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            protected set
            {
                if (_timeRemaining != value)
                {
                    _timeRemaining = value;
                    ProgressChangedEvent?.Invoke(_timeRemaining);
                }
            }
        }

        public ExerciseState State
        {
            get => _state;
            protected set
            {
                if (_state != value)
                {
                    _state = value;
                    StateChangedEvent?.Invoke(_state);
                }
            }
        }

        public event Action? FinishedEvent;
        public event Action<int>? ProgressChangedEvent;
        public event Action<ExerciseState>? StateChangedEvent;

        protected TimedWorkoutElement(WorkoutElements type, int durationSeconds)
        {
            Type = type;
            Duration = durationSeconds;

            _timer = new System.Timers.Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimedEvent;

            State = ExerciseState.Stop;
            TimeRemaining = SlotDuration = durationSeconds;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimedEvent;
            }
            finally
            {
                _timer.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            switch (State)
            {
                case ExerciseState.Stop:
                    StartFromStop();
                    break;

                case ExerciseState.Pause:
                    ResumeFromPause();
                    break;
            }
        }

        public void Stop()
        {
            if (State == ExerciseState.Running || State == ExerciseState.Switch)
            {
                _timer.Stop();
                State = ExerciseState.Pause;
            }
        }

        public void Reset()
        {
            if (State == ExerciseState.Pause)
            {
                _timer.Stop();
                ResetToInitial();
            }
        }

        protected virtual void StartFromStop()
        {
            // default: start a single running slot
            State = ExerciseState.Running;
            _timer.Start();
        }

        protected virtual void ResumeFromPause()
        {
            // keep current TimeRemaining; just resume
            State = ExerciseState.Running;
            _timer.Start();
        }

        protected virtual void ResetToInitial()
        {
            State = ExerciseState.Stop;
            TimeRemaining = SlotDuration = Duration;
        }

        /// <summary>
        /// Helper for derived classes (e.g. Exercise timeslots).
        /// </summary>
        protected void SetSlot(ExerciseState state, int durationSeconds, bool startTimer)
        {
            State = state;
            TimeRemaining = SlotDuration = durationSeconds;

            if (startTimer)
                _timer.Start();
        }

        protected void FinishAndReset()
        {
            _timer.Stop();
            State = ExerciseState.Stop;
            FinishedEvent?.Invoke();
            ResetToInitial();
        }

        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            // keep semantics like your current code:
            // - tick down until 0
            // - slot completion happens on the NEXT tick when it is already 0
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
                return;
            }

            _timer.Stop();
            OnSlotCompleted();
        }

        protected virtual void OnSlotCompleted()
        {
            // default: single-slot element finishes
            FinishAndReset();
        }
    }
}
