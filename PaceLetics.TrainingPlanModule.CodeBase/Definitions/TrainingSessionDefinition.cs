using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Definitions;

public sealed class TrainingSessionDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public List<RunningSessionDefinition> Runs { get; set; } = new();
    public List<WorkoutSessionDefinition> Workouts { get; set; } = new();
    public List<TrainingSessionActivity> Warmup { get; set; } = new();
    public List<TrainingSessionActivity> Drills { get; set; } = new();
    public TrainingEffect? TrainingEffect { get; set; }
    public TrainingSessionAppointment? Appointment { get; set; }
}
