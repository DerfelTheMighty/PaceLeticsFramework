using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Core;
using MudBlazor.Extensions;
using WorkoutModule.Contracts;
using WorkoutModule.Enums;



namespace PaceLetics.Components.Components.Workout
{
    public partial class WorkoutView
    {
        [Parameter]
        public IWorkout Workout { get; set; }

        private int _currentExercise;
        private int _timeRemaining;
        private int _index = -1;
        private double _prgs;
        private double[] _data;
        private ExerciseState _exerciseState;
        private WorkoutElements _elementType;
        private string _instruction;

		protected override void OnInitialized()
        {
            _data = new double[2];
            Workout.ElementFinishedEvent += OnElementFinished;
            Workout.WorkoutFinishedEvent += OnWorkoutFinished;
            Workout.ElementStartEvent += OnElementStart;
            base.OnInitialized();
        }

        private string [] GetChartPalette(WorkoutElements el, ExerciseState state) 
        {
            var palette = new string[] { "", "" };

            if (_elementType == WorkoutElements.Preparation)
            {
				palette = new[] { "#20d2f4", "0bba83ff" };
			}
            else if (_elementType == WorkoutElements.Exercise)
            {
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                    palette = new[] { "#808080", "0bba83ff" };
                else if (_exerciseState == ExerciseState.Switch)
                    palette = new[] { "#FF44CC", "0bba83ff" };
                else
                    palette = new[] { "#FF5E00", "0bba83ff" };
			}
            else if (_elementType == WorkoutElements.Rest) 
            {
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                    palette = new[] { "#808080", "0bba83ff" };
                else 
					palette = new[] { "#7FFF00", "0bba83ff" };
			}
			return palette;
		}

        private string GetInstruction(WorkoutElements el, ExerciseState state) 
        {
            string result = string.Empty;
            if (_elementType == WorkoutElements.Preparation) 
            {
                result = "Bereit machen!";
            }
			else if (_elementType == WorkoutElements.Exercise)
			{
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                    result = "Pausiert!";
                else if (_exerciseState == ExerciseState.Switch)
                    result = "Seitenwechsel!";
                else
                    result = "Los gehts!";
			}
			else if (_elementType == WorkoutElements.Rest)
			{
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                    result = "Pausiert!";
                else
                    result = "Erhohlungspause!";
			}
			return result;
		}


        private void StartWorkout()
        {
            Workout.Start();
        }
        private void StopWorkout() 
        {
            Workout.Stop();
        }
        private void ResetWorkout() 
        {
            Workout.Reset();
        }

		private void OnToggleChanged(bool isToggled)
		{
			if (isToggled)
			{
				StartWorkout();
			}
			else
			{
				StopWorkout(); 
			}
		}

		private void OnElementFinished(IWorkoutElement el)
        {
            el.ProgressChangedEvent -= OnProgressChanged;
        }
        private async void OnWorkoutFinished()
        {
            await InvokeAsync(() =>
            {
                var options = new DialogOptions { CloseOnEscapeKey = true };
                dialogService.Show<WorkoutFinishedDialog>("Herzlichen Glückwünsch! Du hast das Workout erfolgreich abgeschlossen!", options);
            });
        }
		private async void OnElementStart(IWorkoutElement el)
        {
            el.ProgressChangedEvent += OnProgressChanged;

            el.StateChangedEvent += OnElementStateChanged;
            await InvokeAsync(() =>
            {
				_currentExercise = Workout.CurrentExercise;
				_elementType = el.Type;
				_instruction = GetInstruction(_elementType, _exerciseState);
				StateHasChanged();
			});

		}
        private async void OnProgressChanged(int remaining)
        {
            await InvokeAsync(() =>
            {
                _timeRemaining = remaining;
                _data[0] = (double)(Workout.Elements.ElementAt(Workout.CurrentElement).SlotDuration - _timeRemaining);
                _data[1] = _timeRemaining; 
                StateHasChanged();
            });
        }
        private async void OnElementStateChanged(ExerciseState state) 
        {
			await InvokeAsync(() =>
			{
                _exerciseState = state;
                _instruction = GetInstruction(_elementType, _exerciseState);
				StateHasChanged();
			});
		}





	}
}