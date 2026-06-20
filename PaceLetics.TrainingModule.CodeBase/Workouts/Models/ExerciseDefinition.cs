


using PaceLetics.TrainingModule.CodeBase.Workouts.Enums;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Models
{
    public class ExerciseDefinition
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Execution { get; set; } = new();
        public int Duration { get; set; }
        public string ImageFile { get; set; } = "";
        public Level Level { get; set; } = Level.None;
        public bool SwitchLeftRight { get; set; }
        public int SwitchTime { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<ContentReference> ReadMore { get; set; } = new();
        public string Source { get; set; } = "";
        public string OwnerUserId { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }


}
