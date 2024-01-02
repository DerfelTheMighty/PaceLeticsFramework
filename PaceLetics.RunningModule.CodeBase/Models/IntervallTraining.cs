using PaceLetics.RunningModule.CodeBase.Interfaces;
using System.Text;

namespace PaceLetics.RunningModule.CodeBase.Models
{
    public class IntervallTraining : IRun
    {
        /// <summary>
        /// Unique id of the intervall trainings
        /// </summary>
        public string Id { get;  }

        /// <summary>
        /// Public name of the intervall training
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Distance per intervall, e.g. 200m, 400m, 1200m, ...
        /// </summary>
        public List<int> Distances {get;}

        /// <summary>
        /// Recovery between intervals e.g. 200m 400m,...
        /// </summary>
        public List<int> Recovery { get; }

        
        /// <summary>
        /// Intervall pace in min/km
        /// </summary>
        public List<TimeSpan> Paces { get; }

        /// <summary>
        /// Time for running one complete intervall. E.g. 200m intervall @ 3:40 the intervall time is 44s 
        /// </summary>
        public List<TimeSpan> IntervallTime { get; }

        /// <summary>
        /// Time for running one lab. E.g. 1000m intervall @3:40, the labtime is 88s
        /// </summary>
        public List<TimeSpan> LabTime { get; }

        /// <summary>
        /// Number of sets E.g. Sets=3:  3 x ( 5x 200m @ 3:40, 200m Pause), 400m Pause
        /// </summary>
        public int Sets { get; }

        /// <summary>
        /// Recovery between sets
        /// </summary>
        public int SetRecovery { get; }

		public TrainingType Type { get; } 

		public IntervallTraining(string id, string mame, TrainingType type, List<int> distances, List<int> recovery, List<TimeSpan> pace, int sets, int setRecovery) 
        {
            Id = id;
            Name = mame;
            Type = type;
            Distances = distances;
            Recovery = recovery;
            Paces = pace;
            IntervallTime = new List<TimeSpan>();
            LabTime = new List<TimeSpan>();
            Recovery = recovery;
            Sets = sets;
            SetRecovery = setRecovery;

            foreach (var dp in distances.Zip(Paces, Tuple.Create))
            {
                var time = new TimeSpan(0, 0, (int)Math.Round(dp.Item1 * dp.Item2.TotalSeconds / 1000.0));
                IntervallTime.Add(time);

                if (dp.Item1 > 400)
                {
                    LabTime.Add(new TimeSpan(0, 0, (int)Math.Round(400 * dp.Item2.TotalSeconds / 1000.0)));
                }
                else
                    LabTime.Add(time);
            }
            

        }



        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Intervall Training: {Name} ");
            stringBuilder.AppendLine($"({Id})");
            stringBuilder.AppendLine($"Sets: {Sets}, Erholung: {SetRecovery}m");
            stringBuilder.AppendLine("Intervall | Distanz | Pace | Intervallzeit | Labzeit | Erholung");

            for (int i = 0; i < Distances.Count; i++)
            {
                // Die Erholung wird für das letzte Intervall ausgelassen.
                var recoveryText = i < Recovery.Count ? $"{Recovery[i]}m" : "N/A";

                stringBuilder.AppendLine($"{i + 1} | {Distances[i]}m | {Paces[i].ToString(@"mm\:ss")} min/km | {IntervallTime[i].ToString(@"mm\:ss")} | {LabTime[i].ToString(@"mm\:ss")} | {recoveryText}");
            }

            return stringBuilder.ToString();
        }

    }
}
