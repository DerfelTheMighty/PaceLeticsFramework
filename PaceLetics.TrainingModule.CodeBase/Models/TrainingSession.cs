using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.RunningModule.CodeBase.Enums;
using PaceLetics.RunningModule.CodeBase.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Enums;
using PaceLetics.TrainingModule.CodeBase.Interfaces;

namespace PaceLetics.Training.CodeBase.Models
{
	public class TrainingSession : ITrainingSession
	{
		DateTime Time { get; }
		TrainingType Type { get; }
		SessionFokus Focus { get; }
		string Comment { get; }
		public ITraining? Training { get; }

		public TrainingSession(TrainingSessionDefinition def, ITraining training) 
		{
			Time = def.Time;
			Type = def.Type;
			Focus = def.Fokus;
			Comment = def.Comment;
			Training = training;
		}
	}
}
