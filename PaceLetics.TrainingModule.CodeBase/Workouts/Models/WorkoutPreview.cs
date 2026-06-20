

using System.Collections.Generic;
using System.Linq;
using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Models
{
    public class WorkoutPreview
	{


		private readonly List<ExercisePreview> _exercises = new();
		private readonly List<Level> _availableLevels;

		public IReadOnlyCollection<ExercisePreview> Exercises { get => _exercises.AsReadOnly(); }

		public string Name { get; }

		public string Id { get; }

		public string Description { get; }

		public Level Level { get; }

		public int Count { get; }

		public IReadOnlyCollection<Level> AvailableLevels => _availableLevels.AsReadOnly();

		public IReadOnlyCollection<string> Tags { get; }

		public IReadOnlyCollection<ContentReference> ReadMore { get; }


		public WorkoutPreview(WorkoutDefinition def, IExerciseCatalog exerciseCatalog, IEnumerable<Level>? availableLevels = null) 
		{
			Name = def.Name ?? string.Empty;
			Id = def.Id ?? string.Empty;
			Description = def.Description ?? string.Empty;
			Level = def.Level;
			Tags = (def.Tags ?? new()).AsReadOnly();
			ReadMore = (def.ReadMore ?? new()).AsReadOnly();
			foreach (var id in def.Exercises ?? Enumerable.Empty<string>())
				_exercises.Add(exerciseCatalog.GetExercisePreview(id, Level));
			Count = _exercises.Count;

			_availableLevels = (availableLevels != null)
				? availableLevels.Distinct().ToList()
				: new List<Level> { Level };
		}
	}
}
