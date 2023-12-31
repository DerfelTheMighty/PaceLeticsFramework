﻿

using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutProvider : IWorkoutProvider
    {
        private List<WorkoutDefinition> _workouts;
        private List<WorkoutPreview> _previews;
        private IExerciseProvider _provider;
        private IWorkout _activeWorkout;


        public WorkoutProvider(IExerciseProvider provider) 
        {
            _provider = provider;
            _previews = new List<WorkoutPreview>();
            DefinitionFactory factory = new DefinitionFactory();
            _workouts = factory.CreateWorkoutExamples();
            foreach (var workoutdef in _workouts) 
            {
                _previews.Add(new WorkoutPreview(workoutdef, provider));
            }

        }

        public IWorkout GetWorkout(string id)
        {
            var def = _workouts.Find(x => x.Id == id);
            return new Workout(def, _provider);
        }

        public List<string> GetWorkoutIds()
        {
            return _workouts.Select(o => o.Id).ToList();
        }

        public WorkoutPreview GetWorkoutPreview(string id)
        {
            return _previews.Find(x => x.Id == id);
        }

        public void SetActiveWorkout(string id, int sets, int rounds) 
        {
            var def = _workouts.Find(x => x.Id == id);
            if(def !=null)
                _activeWorkout = new Workout(def, _provider, sets, rounds); 
        }

        public IWorkout GetActiveWorkout() 
        {
            return _activeWorkout;
        }
    }
}
