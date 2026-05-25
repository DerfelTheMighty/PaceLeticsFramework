namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed record WorkoutSessionDefinition(
    string WorkoutId,
    string Name = "",
    int Sets = 1,
    int Rounds = 1);
