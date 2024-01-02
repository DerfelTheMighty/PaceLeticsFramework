using CoreLibrary.Constants;
using CoreLibrary.Enums;
using RunningModule.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunningModule.Models
{
	internal class TrainingFactory
	{
		public List<IntervallBlock> CreateTrainingExample()
		{

			List<IntervallBlock> res = new List<IntervallBlock>();
			res.Add(new IntervallBlock()
			{
				Type = TrainingElements.Intervall,
				Distance = 200,
				Pace = new Dictionary<ExperienceLevel, string>()
				{
					{ ExperienceLevel.Novice, PaceKeys.Intervall},
					{ ExperienceLevel.Intermediate, PaceKeys.Repetition}
				},
				Pause = 200,
				Repetitions = 12
			});

			res.Add(new IntervallBlock()
			{
				Type = TrainingElements.Intervall,
				Distance = 800,
				Pace = new Dictionary<ExperienceLevel, string>()
				{
					{ ExperienceLevel.Novice, PaceKeys.Threshold },
					{ ExperienceLevel.Intermediate, PaceKeys.Intervall }
				},
				Pause = 400,
				Repetitions = 6
			});
			return res;
		}
	}
}
