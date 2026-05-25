using PaceLetics.WorkoutModule.CodeBase.Interfaces;
using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Logic;
using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Services
{
    public class WorkoutFactory : IWorkoutFactory
    {
        private readonly IExerciseCatalog _exerciseCatalog;
        private readonly IExerciseFactory _exerciseFactory;

        public WorkoutFactory(IExerciseCatalog exerciseCatalog, IExerciseFactory exerciseFactory)
        {
            _exerciseCatalog = exerciseCatalog;
            _exerciseFactory = exerciseFactory;
        }

        public IWorkout Create(WorkoutDefinition definition)
        {
            return Create(definition, new WorkoutBuildOptions());
        }

        public IWorkout Create(WorkoutDefinition definition, WorkoutBuildOptions options)
        {
            if (options.Sets < 1)
                throw new ArgumentOutOfRangeException(nameof(options), "Sets must be greater than zero.");

            if (options.Rounds < 1)
                throw new ArgumentOutOfRangeException(nameof(options), "Rounds must be greater than zero.");

            return new Workout(definition, BuildElements(definition, options));
        }

        private List<IWorkoutElement> BuildElements(WorkoutDefinition definition, WorkoutBuildOptions options)
        {
            var elements = new List<IWorkoutElement>
            {
                new Rest(definition.PreparationTime, WorkoutElements.Preparation)
            };

            for (int round = 0; round < options.Rounds; round++)
            {
                foreach (var id in definition.Exercises)
                {
                    for (int set = 0; set < options.Sets; set++)
                    {
                        var exerciseDefinition = _exerciseCatalog.GetDefinition(id, definition.Level);
                        elements.Add(_exerciseFactory.Create(exerciseDefinition));
                        elements.Add(new Rest(definition.RestTime, WorkoutElements.Rest));
                    }
                }
            }

            if (elements.Count > 1)
                elements.RemoveAt(elements.Count - 1);

            return elements;
        }
    }
}
