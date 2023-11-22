using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Components.ObjectEdit;
using MudBlazor;
using WorkoutModule.Contracts;
using WorkoutModule.Enums;
using MudBlazor.Extensions.Core;


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

        protected override void OnInitialized()
        {
            _data = new double[2];
            Workout.ElementFinishedEvent += OnElementFinished;
            Workout.WorkoutFinishedEvent += OnWorkoutFinished;
            Workout.ElementStartEvent += OnElementStart;
            StartWorkout();
            base.OnInitialized();
        }

        private void StartWorkout()
        {
            Workout.Start();
        }

        private void OnElementFinished(IWorkoutElement el)
        {
            el.ProgressChangedEvent -= OnProgressChanged;
        }

        private void OnWorkoutFinished()
        {
        }

        private async void OnElementStart(IWorkoutElement el)
        {
            el.ProgressChangedEvent += OnProgressChanged;

            el.StateChangedEvent += OnElementStateChanged;
            await InvokeAsync(() =>
            {
				_currentExercise = Workout.CurrentExercise;
				_elementType = el.Type;
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
				StateHasChanged();
			});
		}

	}
}