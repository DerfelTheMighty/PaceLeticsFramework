using Microsoft.AspNetCore.Components;
using WorkoutModule.Contracts;
using WorkoutModule.Enums;

namespace PaceLetics.Components.Components.Workout
{
    public partial class WorkoutControl
    {
        private int _timeRemaining;
        private int _index = -1;
        private ExerciseState _exerciseState;
        private WorkoutElements _elementType;
        private bool _isToggled;
        private string _instruction;

        private double[] _data;
        [Parameter]
        public IWorkout? Workout { get; set; }

        public bool IsToggled
        {
            get => _isToggled;
            set{
                if (_isToggled != value)
                {
					if (_isToggled)
					{
						StopWorkout();
						_isToggled = value;
					}
					else
					{
						StartWorkout();
						
                        _isToggled = value;
					}
				}
            }
        }

        private void StopWorkout()
        {
            Workout?.Stop();
        }

        private void StartWorkout()
        {
            Workout?.Start();
        }

        private void ResetWorkout()
        {
            Workout?.Reset();
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

        private async void OnElementStart(IWorkoutElement el)
        {
            el.ProgressChangedEvent += OnProgressChanged;
            el.StateChangedEvent += OnElementStateChanged;
            await InvokeAsync(() =>
            {
				_instruction = GetInstruction(_elementType, _exerciseState);
				_elementType = el.Type;
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

        private void OnElementFinished(IWorkoutElement el)
        {
            el.ProgressChangedEvent -= OnProgressChanged;
        }
        
        private void OnWorkoutFinished() 
        {
            InvokeAsync(() => 
            {
				_isToggled = false;
                Workout.Reset();
                StateHasChanged();
			});
            

        }

		private string GetInstruction(WorkoutElements el, ExerciseState state)
		{
			string result = string.Empty;
            if (Workout.State == WorkoutState.Running || Workout.State == WorkoutState.Pause)
            {
                if (_elementType == WorkoutElements.Preparation)
                {
                    result = "Halte dich bereit!";
                }
                else if (_elementType == WorkoutElements.Exercise)
                {
                    if (_exerciseState == ExerciseState.Pause)
                        result = "Workout pausiert!";
                    else if (_exerciseState == ExerciseState.Switch)
                        result = "Seitenwechsel!";
                    else
                        result = "Los gehts!";
                }
                else if (_elementType == WorkoutElements.Rest)
                {
                    if (_exerciseState == ExerciseState.Pause)
                        result = "Workout pausiert!";
                    else
                        result = "Kurze Erholung!";
                }
            }
            else if (Workout.State == WorkoutState.Finished || Workout.State == WorkoutState.Stop)
                result = string.Empty;


			return result;
		}

		private string[] GetChartPalette(WorkoutElements el, ExerciseState state)
        {
            var palette = new string[]{"", ""};
            if (_elementType == WorkoutElements.Preparation)
            {
                palette = new[]{"#20d2f4", "0bba83ff"};
            }
            else if (_elementType == WorkoutElements.Exercise)
            {
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                    palette = new[]{"#808080", "0bba83ff"};
                else if (_exerciseState == ExerciseState.Switch)
                    palette = new[]{"#FF44CC", "0bba83ff"};
                else
                    palette = new[]{"#FF5E00", "0bba83ff"};
            }
            else if (_elementType == WorkoutElements.Rest)
            {
                if (_exerciseState == ExerciseState.Pause || _exerciseState == ExerciseState.Stop)
                    palette = new[]{"#808080", "0bba83ff"};
                else
                    palette = new[]{"#7FFF00", "0bba83ff"};
            }

            return palette;
        }

        protected override void OnInitialized()
        {
            _data = new double[2];
            Workout.ElementFinishedEvent += OnElementFinished;
            Workout.ElementStartEvent += OnElementStart;
            Workout.WorkoutFinishedEvent += OnWorkoutFinished;
            base.OnInitialized();
        }
    }
}