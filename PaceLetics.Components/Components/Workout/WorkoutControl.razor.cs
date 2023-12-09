
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Core;
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
        private ExerciseState _lastState;
        private bool _isToggled;
        private string _instruction;
        private MudExGradientText _grdText;
        private List<MudExColor> _color;
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
                if(remaining==3)
                    JSRuntime.InvokeVoidAsync("PlayTimer");
                StateHasChanged();
            });
        }

        private async void OnElementStart(IWorkoutElement el)
        {
            el.ProgressChangedEvent += OnProgressChanged;
            el.StateChangedEvent += OnElementStateChanged;
            await InvokeAsync(() =>
            {
				_instruction = GetInstruction(_elementType, _exerciseState, Workout.State);
				_elementType = el.Type;
                StateHasChanged();
            });
        }

        private async void OnElementStateChanged(ExerciseState state)
        {
            await InvokeAsync(() =>
            {
                _lastState = _exerciseState;
                _exerciseState = state;

				_instruction = GetInstruction(_elementType, _exerciseState, Workout.State);
                if (GetBeep(_lastState, _exerciseState))
                    JSRuntime.InvokeVoidAsync("PlayDing_1");
                StateHasChanged();
            });
        }

        private async void OnElementFinished(IWorkoutElement el)
        {
            el.ProgressChangedEvent -= OnProgressChanged;
			await JSRuntime.InvokeVoidAsync("PlayDing_1");
		}
        
        private void OnWorkoutFinished() 
        {
            InvokeAsync(() => 
            {
				_isToggled = false;
                Workout.Reset();
                JSRuntime.InvokeVoidAsync("PlayDing_3");
                StateHasChanged();
            });
        }

		private static string GetInstruction(WorkoutElements el, ExerciseState eState, WorkoutState wState)
		{
			string result = string.Empty;
            if (wState == WorkoutState.Running || wState == WorkoutState.Pause)
            {
                if (el == WorkoutElements.Preparation)
                {
                    result = "Bereit machen!";
                }
                else if (el == WorkoutElements.Exercise)
                {
                    if (eState == ExerciseState.Pause)
                        result = "Angehalten!";
                    else if (eState == ExerciseState.Switch)
                        result = "Seitenwechsel!";
                    else
                        result = "Ausführung!";
                }
                else if (el == WorkoutElements.Rest)
                {
                    if (eState == ExerciseState.Pause)
                        result = "Angehalten!";
                    else
                        result = "Übungspause!";
                }
            }
            else if (wState == WorkoutState.Finished || wState == WorkoutState.Stop)
                result = string.Empty;


			return result;
		}

		private static string[] GetChartPalette(WorkoutElements el, ExerciseState state)
        {
            var palette = new string[]{"", ""};
            if (el == WorkoutElements.Preparation)
            {
                palette = new[]{"#20d2f4", "0bba83ff"};
            }
            else if (el == WorkoutElements.Exercise)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    palette = new[]{"#808080", "0bba83ff"};
                else if (state == ExerciseState.Switch)
                    palette = new[]{"#FF44CC", "0bba83ff"};
                else
                    palette = new[]{"#FF5E00", "0bba83ff"};
            }
            else if (el == WorkoutElements.Rest)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    palette = new[]{"#808080", "0bba83ff"};
                else
                    palette = new[]{"#7FFF00", "0bba83ff"};
            }

            return palette;
        }

        private static List<MudExColor> GetGradTextPallette(WorkoutElements el, ExerciseState state)
        {
            List<MudExColor> color = new List<MudExColor> { MudExColor.Info, MudExColor.Dark };
            if (el == WorkoutElements.Preparation)
            {
                color = new List<MudExColor> { MudExColor.Info, MudExColor.Dark };
            }
            else if (el == WorkoutElements.Exercise)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    color = new List<MudExColor> { MudExColor.Surface, MudExColor.Dark };
                else if (state == ExerciseState.Switch)
                    color = new List<MudExColor> { MudExColor.Primary, MudExColor.Dark };
                else
                    color = new List<MudExColor> { MudExColor.Error, MudExColor.Dark };
            }
            else if (el == WorkoutElements.Rest)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    color = new List<MudExColor> { MudExColor.Surface, MudExColor.Dark };
                else
                    color = new List<MudExColor> { MudExColor.Success, MudExColor.Dark };


            }


            return color;
        }

        private static bool GetBeep(ExerciseState lastState, ExerciseState state) 
        {
            if (state == ExerciseState.Switch || lastState == ExerciseState.Switch)
                return true;
            else
                return false;
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