using Microsoft.AspNetCore.Components;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;


namespace PaceLetics.TrainingModule.Components.Workouts
{
    public partial class WorkoutView : IDisposable
    {
        [Parameter]
        public IWorkout Workout { get; set; } = default!;


        private bool _allowSlide;
        private int _currentExercise;
        private ExerciseState _exerciseState;
        private WorkoutElements _elementType;
        public bool IsControlExpanded { get; set; }
        public string ControlText 
        {
            get 
            {
                if (IsControlExpanded)
                    return "Workoutsteuerung ausblenden";
                else
                    return "Workoutsteuerung einblenden";
            }
        }

		protected override void OnInitialized()
        {
            IsControlExpanded = true;
            _allowSlide = true;
            Workout.ElementFinishedEvent += OnElementFinished;
            Workout.WorkoutFinishedEvent += OnWorkoutFinished;
            Workout.ElementStartEvent += OnElementStart;
            Workout.WorkoutStartEvent += OnWorkoutStart;
            base.OnInitialized();
        }

        


		private void OnElementFinished(IWorkoutElement el)
        {
            el.StateChangedEvent -= OnElementStateChanged;
        }

		private void OnElementStart(IWorkoutElement el)
        {
            el.StateChangedEvent += OnElementStateChanged;
            _ = InvokeAsync(() =>
            {
				_currentExercise = Workout.CurrentExercise;
				_elementType = el.Type;		
				StateHasChanged();
			});
		}

        private void OnElementStateChanged(ExerciseState state)
        {
			_ = InvokeAsync(() =>
			{
                _exerciseState = state;
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                {
                    _allowSlide = true;
                }
                else 
                {
                    _allowSlide = false;
                }
				StateHasChanged();
			});
		}

        private void OnWorkoutFinished()
        {
            _ = InvokeAsync(() =>
            {
                _allowSlide = true;
            });
        }

        private void OnWorkoutStart()
        {
            _ = InvokeAsync(() =>
            {
                _allowSlide = false;
            });
        }


        private bool _sheetOpen;

        private Task CloseSheet()
        {
            _sheetOpen = false;
            return Task.CompletedTask;
        }

        private Task OpenSheet()
        {
            _sheetOpen = true;
            return Task.CompletedTask;
        }

        private void OnSelectIndex(int idx)
        {
            if (!_allowSlide) return;    // Running => keine Navigation
            if (Workout?.Exercises is null) return;
            if (idx < 0 || idx >= Workout.Exercises.Count) return;

            _currentExercise = idx;
            StateHasChanged();
        }

        public void Dispose()
        {
            Workout.ElementFinishedEvent -= OnElementFinished;
            Workout.WorkoutFinishedEvent -= OnWorkoutFinished;
            Workout.ElementStartEvent -= OnElementStart;
            Workout.WorkoutStartEvent -= OnWorkoutStart;
            foreach (var element in Workout.Elements)
                element.StateChangedEvent -= OnElementStateChanged;
        }

    }
}
