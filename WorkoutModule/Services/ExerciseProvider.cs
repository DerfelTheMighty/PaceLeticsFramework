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
    public class ExerciseProvider : IExerciseProvider
    {


        private List<Exercise> _exercises;
        public ExerciseProvider()
        {
            _exercises = new List<Exercise>();
            DefinitionFactory defFactory = new DefinitionFactory();

            var exercisDefs = defFactory.CreateExerciseExamples();

            foreach (var def in exercisDefs)
            {
                _exercises.Add(new Exercise(def));
            }
        }


        /// <summary>
        /// Returns exercise by id. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Exercise GetExercise(string id)
        {
            return _exercises.Find(x => x.Id == id);
        }

        /// <summary>
        /// Returns a list of all availabe exercises by id
        /// </summary>
        /// <returns></returns>
        public List<string> GetExerciseIds()
        {
            return _exercises.Select(o => o.Id).ToList();
        }


    }
}
