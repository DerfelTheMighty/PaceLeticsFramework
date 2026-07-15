using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.Web.Services.TrainingPresentation;

public enum TrainingIntensity
{
    Easy,
    Moderate,
    High
}

public static class TrainingSessionPresentation
{
    public static int EstimateDurationMinutes(TrainingSession session)
    {
        if (session.Appointment.StartsAt is not null && session.Appointment.EndsAt is not null)
        {
            return Math.Max(5, (int)Math.Round(
                (session.Appointment.EndsAt.Value - session.Appointment.StartsAt.Value).TotalMinutes));
        }

        var activitySeconds = session.Warmup.Sum(item => item.DurationSeconds)
            + session.Drills.Sum(item => item.DurationSeconds);
        var runMinutes = session.TotalRunDistance > 0
            ? session.TotalRunDistance / 1000.0 * 6.0
            : 0;
        var workoutMinutes = session.Workouts.Sum(workout =>
            Math.Max(12, 12 * Math.Max(1, workout.Sets) * Math.Max(1, workout.Rounds)));
        var total = activitySeconds / 60.0 + runMinutes + workoutMinutes;

        if (total <= 0)
            total = 30;

        return Math.Max(5, (int)(Math.Round(total / 5.0) * 5));
    }

    public static TrainingIntensity GetIntensity(TrainingSession session)
    {
        var effectText = $"{session.TrainingEffect.Focus} {session.TrainingEffect.Stimulus}"
            .ToLowerInvariant();
        var hasFastRunning = session.Runs
            .SelectMany(run => run.Sequence)
            .Any(segment => segment.Type is SegmentType.Intervall or SegmentType.SetRecovery);

        if (hasFastRunning
            || effectText.Contains("tempo", StringComparison.Ordinal)
            || effectText.Contains("threshold", StringComparison.Ordinal)
            || effectText.Contains("speed", StringComparison.Ordinal)
            || effectText.Contains("intens", StringComparison.Ordinal))
        {
            return TrainingIntensity.High;
        }

        if (session.Workouts.Any(workout => workout.Rounds > 1 || workout.Sets > 1)
            || session.TotalRunDistance >= 8000)
        {
            return TrainingIntensity.Moderate;
        }

        return TrainingIntensity.Easy;
    }

    public static string GetPurpose(TrainingSession session)
    {
        if (!string.IsNullOrWhiteSpace(session.TrainingEffect.Focus))
            return session.TrainingEffect.Focus;
        if (!string.IsNullOrWhiteSpace(session.TrainingEffect.Adaptation))
            return session.TrainingEffect.Adaptation;
        if (session.Runs.SelectMany(run => run.Sequence).Any(segment => segment.Type == SegmentType.Intervall))
            return "speed";
        if (session.Workouts.Count > 0 && session.Runs.Count == 0)
            return "strength";
        return "endurance";
    }

    public static string GetComposition(TrainingSession session)
    {
        var parts = new List<string>();
        if (session.TotalRunDistance > 0)
            parts.Add($"{session.TotalRunDistance / 1000.0:0.#} km");
        if (session.Workouts.Count > 0)
            parts.Add(session.Workouts.Count == 1 ? session.Workouts[0].Name : $"{session.Workouts.Count} Workouts");
        if (session.Warmup.Count > 0)
            parts.Add("Warm-up");
        if (session.Drills.Count > 0)
            parts.Add("Drills");
        return parts.Count == 0 ? session.Name : string.Join(" · ", parts);
    }
}
