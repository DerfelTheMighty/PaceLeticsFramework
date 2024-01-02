

using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class ExerciseProvider : IExerciseProvider
    {

        private List<Exercise> _exercises;

        private List<ExercisePreview> _previews;
        public ExerciseProvider()
        {
            _exercises = new List<Exercise>();
            _previews = new List<ExercisePreview>();
            DefinitionFactory defFactory = new DefinitionFactory();

            var exercisDefs = defFactory.CreateExerciseExamples();

            foreach (var def in exercisDefs)
            {
                _exercises.Add(new Exercise(def));
                _previews.Add(new ExercisePreview(def));    
            }
        }


        /// <summary>
        /// Returns exercise by id. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Exercise GetExercise(string id, Level lvl)
        {
            return _exercises.Find(x => x.Id == id && x.Level == lvl);
        }

        public ExercisePreview GetExercisePreview(string id, Level lvl) 
        {
            return _previews.Find(x => x.Id == id && x.Level == lvl);
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
