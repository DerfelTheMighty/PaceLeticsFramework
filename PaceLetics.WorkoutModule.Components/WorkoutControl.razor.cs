using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;

using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Core;

namespace PaceLetics.WorkoutModule.Components
{
    public partial class WorkoutControl : IDisposable
    {
        private int _timeRemaining;
        private int _index = -1;
        private ExerciseState _exerciseState;
        private WorkoutElements _elementType;
        private ExerciseState _lastState;
        private bool _isToggled;
        private string _instruction = string.Empty;
        private MudExGradientText _grdText;
        private List<MudExColor> _color;
        private double[] _data;

        private IWorkout? _subscribedWorkout;

        [Parameter]
        public IWorkout? Workout { get; set; }

        public bool IsToggled
        {
            get => _isToggled;
            set
            {
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

        private void StopWorkout() => Workout?.Stop();
        private void StartWorkout() => Workout?.Start();
        private void ResetWorkout() => Workout?.Reset();

        protected override void OnInitialized()
        {
            _data = new double[2];
            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            // Wenn der Parent ein neues Workout reinreicht -> altes sauber abklemmen
            if (!ReferenceEquals(_subscribedWorkout, Workout))
            {
                UnsubscribeFromWorkout(_subscribedWorkout);
                SubscribeToWorkout(Workout);
                _subscribedWorkout = Workout;

                // Optional: UI-Status zurücksetzen, damit du keine "alten" Anzeigezustände siehst
                _timeRemaining = 0;
                _exerciseState = ExerciseState.Stop;
                _lastState = ExerciseState.Stop;
                _elementType = default;
                _instruction = string.Empty;
                _data[0] = 0;
                _data[1] = 0;
                _isToggled = false;
            }

            base.OnParametersSet();
        }

        private void SubscribeToWorkout(IWorkout? w)
        {
            if (w is null) return;

            w.ElementFinishedEvent += OnElementFinished;
            w.ElementStartEvent += OnElementStart;
            w.WorkoutFinishedEvent += OnWorkoutFinished;
        }

        private void UnsubscribeFromWorkout(IWorkout? w)
        {
            if (w is null) return;

            w.ElementFinishedEvent -= OnElementFinished;
            w.ElementStartEvent -= OnElementStart;
            w.WorkoutFinishedEvent -= OnWorkoutFinished;

            // Falls gerade ein Element aktiv war, sicherheitshalber dessen Events lösen
            try
            {
                if (w.Elements != null && w.CurrentElement >= 0 && w.CurrentElement < w.Elements.Count)
                {
                    var el = w.Elements.ElementAt(w.CurrentElement);
                    el.ProgressChangedEvent -= OnProgressChanged;
                    el.StateChangedEvent -= OnElementStateChanged;
                }
            }
            catch
            {
                // bewusst ignorieren: UI cleanup soll nie crashen
            }

            // Option B: Workout besitzt Timer -> hier zentral entsorgen,
            // sobald es für diese Komponente "weg" ist.
            (w as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            UnsubscribeFromWorkout(_subscribedWorkout);
            _subscribedWorkout = null;
        }

        private async void OnProgressChanged(int remaining)
        {
            await InvokeAsync(() =>
            {
                _timeRemaining = remaining;

                // Achtung: _elementType muss schon gesetzt sein, sonst ist SlotDuration ggf. falsch.
                var current = Workout?.Elements?.ElementAtOrDefault(Workout.CurrentElement);
                if (current != null)
                {
                    _data[0] = (double)(current.SlotDuration - _timeRemaining);
                    _data[1] = _timeRemaining;
                }
                else
                {
                    _data[0] = 0;
                    _data[1] = 0;
                }

                if (remaining == 3)
                    JSRuntime.InvokeVoidAsync("PlayTimer");

                StateHasChanged();
            });
        }

        private async void OnElementStart(IWorkoutElement el)
        {
            // erst Typ übernehmen, dann Instruction berechnen
            _elementType = el.Type;

            el.ProgressChangedEvent += OnProgressChanged;
            el.StateChangedEvent += OnElementStateChanged;

            await InvokeAsync(() =>
            {
                _instruction = GetInstruction(_elementType, _exerciseState, Workout?.State ?? WorkoutState.Stop);
                StateHasChanged();
            });
        }

        private async void OnElementStateChanged(ExerciseState state)
        {
            await InvokeAsync(() =>
            {
                _lastState = _exerciseState;
                _exerciseState = state;

                _instruction = GetInstruction(_elementType, _exerciseState, Workout?.State ?? WorkoutState.Stop);

                if (GetBeep(_lastState, _exerciseState))
                    JSRuntime.InvokeVoidAsync("PlayDing_1");

                StateHasChanged();
            });
        }

        private async void OnElementFinished(IWorkoutElement el)
        {
            // WICHTIG: beide Events lösen (StateChanged fehlte vorher)
            el.ProgressChangedEvent -= OnProgressChanged;
            el.StateChangedEvent -= OnElementStateChanged;

            await JSRuntime.InvokeVoidAsync("PlayDing_1");
        }

        private void OnWorkoutFinished()
        {
            InvokeAsync(() =>
            {
                _isToggled = false;

                // Achtung: Reset ist ok, aber wenn du im Parent nach WorkoutFinished ein neues Workout setzt,
                // übernimmt OnParametersSet das Umschalten & Cleanup.
                Workout?.Reset();

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
            {
                result = string.Empty;
            }

            return result;
        }

        private static string[] GetChartPalette(WorkoutElements el, ExerciseState state)
        {
            var palette = new[] { "", "" };

            if (el == WorkoutElements.Preparation)
            {
                palette = new[] { "#20d2f4", "0bba83ff" };
            }
            else if (el == WorkoutElements.Exercise)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    palette = new[] { "#808080", "0bba83ff" };
                else if (state == ExerciseState.Switch)
                    palette = new[] { "#FF44CC", "0bba83ff" };
                else
                    palette = new[] { "#FF5E00", "0bba83ff" };
            }
            else if (el == WorkoutElements.Rest)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    palette = new[] { "#808080", "0bba83ff" };
                else
                    palette = new[] { "#7FFF00", "0bba83ff" };
            }

            return palette;
        }

        private static List<MudExColor> GetGradTextPallette(WorkoutElements el, ExerciseState state)
        {
            List<MudExColor> color = new() { MudExColor.Info, MudExColor.Dark };

            if (el == WorkoutElements.Preparation)
            {
                color = new() { MudExColor.Info, MudExColor.Dark };
            }
            else if (el == WorkoutElements.Exercise)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    color = new() { MudExColor.Surface, MudExColor.Dark };
                else if (state == ExerciseState.Switch)
                    color = new() { MudExColor.Primary, MudExColor.Dark };
                else
                    color = new() { MudExColor.Error, MudExColor.Dark };
            }
            else if (el == WorkoutElements.Rest)
            {
                if (state == ExerciseState.Pause || state == ExerciseState.Stop)
                    color = new() { MudExColor.Surface, MudExColor.Dark };
                else
                    color = new() { MudExColor.Success, MudExColor.Dark };
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
    }

    internal static class EnumerableExtensions
    {
        public static T? ElementAtOrDefault<T>(this IEnumerable<T> source, int index)
        {
            if (source == null) return default;
            if (index < 0) return default;

            using var e = source.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!e.MoveNext())
                    return default;
            }
            return e.Current;
        }
    }
}
