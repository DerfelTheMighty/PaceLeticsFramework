using PaceLetics.RunningModule.CodeBase.Enums;
using PaceLetics.TrainingModule.CodeBase.Enums;

namespace PaceLetics.Training.CodeBase.Models
{
	public class TrainingSessionDefinition
	{
		public DateTime Time { get; set; }

		public TrainingType Type { get; set; }

		public SessionFokus Fokus { get; set; }

		public string Comment { get; set; }


		public TrainingSessionDefinition() 
		{
			Comment = string.Empty;
		}

	}
}
