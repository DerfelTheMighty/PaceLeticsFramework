

using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutFactory
    {
        public WorkoutFactory() 
        {
        }

        public Workout CreateWorkout(WorkoutDefinition def, IExerciseProvider exProvider) 
        {
            Workout workout = new Workout(def, exProvider);
            return workout;

        }
    }
}
