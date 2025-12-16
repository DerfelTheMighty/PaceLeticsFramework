


using PaceLetics.WorkoutModule.CodeBase.Enums;

namespace PaceLetics.WorkoutModule.CodeBase.Models
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
    }


}
