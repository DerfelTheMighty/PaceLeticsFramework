using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutModule.Contracts;
using WorkoutModule.Logic;
using WorkoutModule.Models;

namespace WorkoutModule.Services
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
