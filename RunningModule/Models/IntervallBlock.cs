using CoreLibrary.Enums;
using RunningModule.Enums;

namespace RunningModule.Models
{
	internal class IntervallBlock
	{
		public TrainingElements Type { get; set; }

		public int Repetitions { get; set; }

		public int Distance { get; set; }

		public int Pause { get; set; }

		public Dictionary<ExperienceLevel, string>? Pace { get; set; }

		

	}
}
