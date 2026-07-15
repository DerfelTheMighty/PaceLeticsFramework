using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services.TrainingPresentation;

namespace PaceLetics.Tests;

public sealed class TrainingSessionPresentationTests
{
    [Fact]
    public void EstimateDurationMinutes_UsesAppointmentWhenAvailable()
    {
        var session = CreateWorkoutSession(
            appointment: new TrainingSessionAppointment(
                new DateTime(2026, 7, 15, 18, 0, 0),
                new DateTime(2026, 7, 15, 19, 15, 0)));

        Assert.Equal(75, TrainingSessionPresentation.EstimateDurationMinutes(session));
    }

    [Fact]
    public void GetIntensity_UsesTrainingEffectForHighIntensitySession()
    {
        var session = CreateWorkoutSession(
            effect: new TrainingEffect(Focus: "Tempo and speed"));

        Assert.Equal(TrainingIntensity.High, TrainingSessionPresentation.GetIntensity(session));
    }

    [Fact]
    public void GetComposition_ExplainsWorkoutAndPreparation()
    {
        var session = new TrainingSession(
            "session-1",
            "Strength",
            new DateTime(2026, 7, 15),
            Array.Empty<PaceLetics.TrainingModule.CodeBase.Running.Models.RunningSession>(),
            new[] { new WorkoutSessionDefinition("core", "Core", 1, 1) },
            new[] { new TrainingSessionActivity("Mobilize", DurationSeconds: 300) },
            Array.Empty<TrainingSessionActivity>(),
            TrainingEffect.Empty,
            TrainingSessionAppointment.Empty);

        var composition = TrainingSessionPresentation.GetComposition(session);

        Assert.Contains("Core", composition);
        Assert.Contains("Warm-up", composition);
    }

    private static TrainingSession CreateWorkoutSession(
        TrainingEffect? effect = null,
        TrainingSessionAppointment? appointment = null)
    {
        return new TrainingSession(
            "session-1",
            "Strength",
            new DateTime(2026, 7, 15),
            Array.Empty<PaceLetics.TrainingModule.CodeBase.Running.Models.RunningSession>(),
            new[] { new WorkoutSessionDefinition("strength", "Strength", 2, 1) },
            Array.Empty<TrainingSessionActivity>(),
            Array.Empty<TrainingSessionActivity>(),
            effect ?? TrainingEffect.Empty,
            appointment ?? TrainingSessionAppointment.Empty);
    }
}
