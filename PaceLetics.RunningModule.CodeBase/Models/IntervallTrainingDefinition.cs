using PaceLetics.RunningModule.CodeBase.Enums;

namespace PaceLetics.RunningModule.CodeBase.Models
{
    public class IntervallTrainingDefinition
    {
        
        public string Id { get; set; }

        public string Name { get; set; }

        public TrainingType Type { get; }

        public List<int> Distances { get; set; }

        public List<int> Recovery { get; set; }

        public List<TimeSpan> Paces { get; set; }

        public int Sets { get; set; }

        public int SetRecovery { get; set; }

        public IntervallTrainingDefinition() 
        {
            Id = string.Empty;
            Name = string.Empty;
            Distances = new List<int>();
            Recovery = new List<int>();
            Paces = new List<TimeSpan>();
        }

    }

}
