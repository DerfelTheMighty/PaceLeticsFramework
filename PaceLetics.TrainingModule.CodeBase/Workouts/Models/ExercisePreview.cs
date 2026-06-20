

using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Models
{
	public class ExercisePreview
	{

		public string Name { get; }

		public string Id { get; }

		public string Description { get; }

		public Level Level { get; }

		public string Imagefile { get; }

		public IReadOnlyCollection<string> Tags { get; }

		public IReadOnlyCollection<ContentReference> ReadMore { get; }

		public ExercisePreview(ExerciseDefinition def) 
		{
			Name = def.Name ?? string.Empty;
			Id = def.Id ?? string.Empty;
			Description = def.Description ?? string.Empty;
			Imagefile = def.ImageFile ?? string.Empty;
			Level = def.Level;
			Tags = (def.Tags ?? new()).AsReadOnly();
			ReadMore = (def.ReadMore ?? new()).AsReadOnly();
		}

	}
}
