using PaceLetics.RunningModule.CodeBase.Models;


namespace PaceLetics.TrainingModule.CodeBase.Models
{
    public class TrainingSchedule
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<TrainingSession> Sessions{get;set;}
    }
}
