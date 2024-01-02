using Microsoft.AspNetCore.Components;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Enums;


namespace PaceLetics.WorkoutModule.Components
{
    public partial class WorkoutView
    {
        [Parameter]
        public IWorkout Workout { get; set; }


        private bool _allowSlide;
        private int _currentExercise;
        private double _prgs;
        private int _toggled;

        private ExerciseState _exerciseState;
        private WorkoutElements _elementType;
        private string _instruction;
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

		private async void OnElementStart(IWorkoutElement el)
        {
            el.StateChangedEvent += OnElementStateChanged;
            await InvokeAsync(() =>
            {
				_currentExercise = Workout.CurrentExercise;
				_elementType = el.Type;		
				StateHasChanged();
			});
		}

        private async void OnElementStateChanged(ExerciseState state) 
        {
			await InvokeAsync(() =>
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

        private async void OnWorkoutFinished() 
        {
            await InvokeAsync(() =>
            {
                _allowSlide = true;
            });
        }

        private async void OnWorkoutStart() 
        {
            await InvokeAsync(() =>
            {
                _allowSlide = false;
            });
        }

	}
}