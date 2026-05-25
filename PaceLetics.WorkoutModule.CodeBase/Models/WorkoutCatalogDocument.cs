namespace PaceLetics.WorkoutModule.CodeBase.Models
{
    public sealed class WorkoutCatalogDocument
    {
        public int SchemaVersion { get; set; }
        public List<ExerciseDefinition> Exercises { get; set; } = new();
        public List<WorkoutDefinition> Workouts { get; set; } = new();
    }
}
