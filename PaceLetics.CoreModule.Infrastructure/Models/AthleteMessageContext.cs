namespace PaceLetics.CoreModule.Infrastructure.Models;

public sealed record AthleteMessageContext(
    string? AthleteId,
    string AthleteName,
    double Vdot,
    RaceResultModel? ActiveReferenceResult,
    string? SelectedTrainingPlanId,
    DateTime Today);
