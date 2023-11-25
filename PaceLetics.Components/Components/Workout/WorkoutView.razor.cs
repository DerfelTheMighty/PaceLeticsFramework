using Microsoft.AspNetCore.Components;
using MudBlazor;

using WorkoutModule.Contracts;
using WorkoutModule.Enums;



namespace PaceLetics.Components.Components.Workout
{
    public partial class WorkoutView
    {
        [Parameter]
        public IWorkout Workout { get; set; }

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

            Workout.ElementFinishedEvent += OnElementFinished;
            
            Workout.ElementStartEvent += OnElementStart;
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
				StateHasChanged();
			});
		}





	}
}