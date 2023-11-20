using WorkoutModule.Enums;

namespace WorkoutModule.Models
{
	public class ExercisePreview
	{
		public string Id { get; }

		public string Description { get; }

		public Level Level { get; }

		public string Imagefile { get; }

		public ExercisePreview(ExerciseDefinition def) 
		{
			Id = def.Id ?? string.Empty;
			Description = def.Description ?? string.Empty;
			Imagefile = def.ImageFile ?? string.Empty;
			Level = def.Level;
		}

	}
}
