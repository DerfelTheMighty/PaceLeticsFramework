namespace PaceLetics.TrainingModule.CodeBase.Workouts.Models
{
    public sealed class WorkoutCatalogDocument
    {
        public int SchemaVersion { get; set; }
        public List<ExerciseDefinition> Exercises { get; set; } = new();
        public List<WorkoutDefinition> Workouts { get; set; } = new();
    }
}
