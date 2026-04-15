

using System.Collections.Generic;
using System.Linq;
using PaceLetics.WorkoutModule.CodeBase.Enums;
using PaceLetics.WorkoutModule.CodeBase.Interfaces;

namespace PaceLetics.WorkoutModule.CodeBase.Models
{
    public class WorkoutPreview
	{


		private List<ExercisePreview> _exercises;
		private List<Level> _availableLevels;

		public IReadOnlyCollection<ExercisePreview> Exercises { get => _exercises.AsReadOnly(); }

		public string Name { get; }

		public string Id { get; }

		public string Description { get; }

		public Level Level { get; }

		public int Count { get; }

		public IReadOnlyCollection<Level> AvailableLevels => _availableLevels.AsReadOnly();


		public WorkoutPreview(WorkoutDefinition def, IExerciseProvider provider, IEnumerable<Level>? availableLevels = null) 
		{
			Name = def.Name ?? string.Empty;
			Id = def.Id ?? string.Empty;
			Description = def.Description ?? string.Empty;
			Level = def.Level;
			_exercises = new List<ExercisePreview>();
			foreach (var id in def.Exercises ?? Enumerable.Empty<string>())
				_exercises.Add(provider.GetExercisePreview(id, Level));
			Count = _exercises?.Count() ?? 0;

			_availableLevels = (availableLevels != null)
				? availableLevels.Distinct().ToList()
				: new List<Level> { Level };
		}
	}
}
