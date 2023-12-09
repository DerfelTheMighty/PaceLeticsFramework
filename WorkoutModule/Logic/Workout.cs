using System.Runtime.CompilerServices;
using WorkoutModule.Contracts;
using WorkoutModule.Enums;
using WorkoutModule.Models;

namespace WorkoutModule.Logic
{
    public class Workout : IWorkout
    {
        private int _currentElement;
        private int _currentExercise;
        private List<IWorkoutElement> _workoutElements;


        #region public properties

        /// <summary>
        /// List containing all exercises in the workout
        /// </summary>
        public List<IExerciseInfo> Exercises { get; }

        public IReadOnlyCollection<IWorkoutElement> Elements 
        {
            get 
            {
                return _workoutElements.AsReadOnly();
            }
        }

        public string Name { get; }

        /// <summary>
        /// Unique id of the workout
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Short description of the workout
        /// </summary>
        public string Description { get; }


        public string Imagefile { get; }

        /// <summary>
        /// Difficulty level of the exercise
        /// </summary>
        public Level Level { get; }

        /// <summary>
        /// Time before the first exercise starts
        /// </summary>
        public int PreparationTime { get; }
        /// <summary>
        /// Rest time between two exercises
        /// </summary>
        public int RestTime { get; }

        /// <summary>
        /// Index of the currently active exercise
        /// </summary>
        public int CurrentElement
        {
            get => _currentElement;
            private set
            {
                if (_currentElement != value)
                {
                    _currentElement = value;
                }
            }
        }
        /// <summary>
        /// The list element containing the currently active exercise
        /// </summary>
        public int CurrentExercise
        {
            get => _currentExercise;
            private set
            {
                if (_currentExercise != value)
                {
                    _currentExercise = value;
                }
            }
        }

        /// <summary>
        /// Workout state 
        /// </summary>
        public WorkoutState State { get; private set; }

        #endregion


        #region Events
        /// <summary>
        /// Event is fired when all workout elements are done
        /// </summary>
        public Action WorkoutFinishedEvent { get; set; }
        /// <summary>
        /// Event is fired whenever a workout element is finished or cancelled
        /// </summary>
        public Action<IWorkoutElement> ElementFinishedEvent { get; set; }
        /// <summary>
        /// Event is fired when the execution of a new workout element start
        /// </summary>
        public Action<IWorkoutElement> ElementStartEvent { get; set; }
        /// <summary>
        /// Event is fired when the workout is started
        /// </summary>
        public Action WorkoutStartEvent { get; set; }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="provider"></param>
        public Workout(WorkoutDefinition definition, IExerciseProvider provider)
        {
            Name = definition.Name ?? string.Empty;
            Id = definition.Id ?? string.Empty;
            Description = definition.Description ?? string.Empty;
            Level = definition.Level;
            PreparationTime = definition.PreparationTime;
            RestTime = definition.RestTime;
            State = WorkoutState.Stop;
            _currentElement = 0;
            _workoutElements = new List<IWorkoutElement>();
            _workoutElements.Add(new Rest(PreparationTime, WorkoutElements.Preparation));
            foreach (var ex in definition.Exercises)
            {
                _workoutElements.Add(provider.GetExercise(ex, Level));
                _workoutElements.Add(new Rest(RestTime, WorkoutElements.Rest));
            }
            _workoutElements.RemoveAt(_workoutElements.Count - 1);
            Exercises = _workoutElements.Where(x => x.Type == WorkoutElements.Exercise).Cast<IExerciseInfo>().ToList();
        }

        public Workout(WorkoutDefinition definition, IExerciseProvider provider, int sets, int rounds)
        {
            Name = definition.Name ?? string.Empty;
            Id = definition.Id ?? string.Empty;
            Description = definition.Description ?? string.Empty;
            Level = definition.Level;
            PreparationTime = definition.PreparationTime;
            RestTime = definition.RestTime;
            State = WorkoutState.Stop;
            _currentElement = 0;
            _workoutElements = new List<IWorkoutElement>();
            _workoutElements.Add(new Rest(PreparationTime, WorkoutElements.Preparation));
            for (int j = 0; j < rounds; j++)
            {
                foreach (var ex in definition.Exercises)
                {
                    for (int i = 0; i < sets; i++)
                    {
                        _workoutElements.Add(provider.GetExercise(ex, Level));
                        _workoutElements.Add(new Rest(RestTime, WorkoutElements.Rest));
                    }
                }
            }
            _workoutElements.RemoveAt(_workoutElements.Count - 1);
            Exercises = _workoutElements.Where(x => x.Type == WorkoutElements.Exercise).Cast<IExerciseInfo>().ToList();
        }

        #region public methods

        /// <summary>
        /// Starts a new workout or resumes a paused workout
        /// </summary>
        public void Start()
        {

            if (State == WorkoutState.Stop || State == WorkoutState.Finished)
            {
                State = WorkoutState.Running;
                WorkoutStartEvent.Invoke();
                ProcessTimeSlot();
            }
            else if (State == WorkoutState.Pause)
            {
                State = WorkoutState.Running;
                _workoutElements[_currentElement].Start();
            }

        }


        /// <summary>
        /// Pauses the workout by halting the currently active workout element
        /// </summary>
        public void Stop()
        {
            if (State == WorkoutState.Running)
            {
                State = WorkoutState.Pause;
                _workoutElements[_currentElement].Stop();
            }

        }

        /// <summary>
        /// Resets the currently paused workout and the currently paused workout element
        /// </summary>
        public void Reset()
        {
            if (State == WorkoutState.Pause)
            {
                State = WorkoutState.Stop;
                _workoutElements[_currentElement].Reset();
                _workoutElements[_currentElement].FinishedEvent -= OnElementFinished;
                ElementFinishedEvent.Invoke(_workoutElements[_currentElement]);
                CurrentElement = 0;
                CurrentExercise = 0;
            }
        }

        #endregion


        #region Private methos

        private void ProcessTimeSlot()
        {

            if (_currentElement < _workoutElements.Count)
            {
                _workoutElements[_currentElement].FinishedEvent += OnElementFinished;
                ElementStartEvent?.Invoke(_workoutElements[_currentElement]);
                _workoutElements[_currentElement].Start();
            }
            else
            {
                FinishWorkout();
            }
        }

        private void FinishWorkout()
        {
            State = WorkoutState.Finished;
            WorkoutFinishedEvent?.Invoke();
            CurrentElement = 0;
            CurrentExercise = 0;
        }



        private void OnElementFinished()
        {
            _workoutElements[_currentElement].FinishedEvent -= OnElementFinished;
            if (_workoutElements[_currentElement].Type == WorkoutElements.Exercise)
                CurrentExercise++;
            ElementFinishedEvent?.Invoke(_workoutElements[_currentElement]);
            CurrentElement++;
            ProcessTimeSlot();
        }

        #endregion
    }

}
