using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Services;

public sealed class TrainingPlanDefinitionValidator : ITrainingPlanDefinitionValidator
{
    private readonly IWorkoutCatalog? _workoutCatalog;

    public TrainingPlanDefinitionValidator(IWorkoutCatalog? workoutCatalog = null)
    {
        _workoutCatalog = workoutCatalog;
    }

    public void Validate(TrainingPlanDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var errors = new List<string>();

        if (definition.SchemaVersion is < 0 or > 1)
            errors.Add($"Training plan '{definition.Id}' has unsupported schemaVersion '{definition.SchemaVersion}'.");

        if (string.IsNullOrWhiteSpace(definition.Id))
            errors.Add("Training plan id must not be empty.");

        if (string.IsNullOrWhiteSpace(definition.Name))
            errors.Add($"Training plan '{definition.Id}' name must not be empty.");

        if (definition.Sessions.Count == 0)
            errors.Add($"Training plan '{definition.Id}' must contain at least one session.");

        foreach (var session in definition.Sessions)
            ValidateSession(definition, session, errors);

        if (errors.Count > 0)
        {
            throw new TrainingPlanDefinitionValidationException(
                $"Training plan definition '{definition.Id}' is invalid.",
                errors);
        }
    }

    private void ValidateSession(
        TrainingPlanDefinition plan,
        TrainingSessionDefinition session,
        List<string> errors)
    {
        var label = string.IsNullOrWhiteSpace(session.Id)
            ? $"Training plan '{plan.Id}' session '{session.Name}'"
            : $"Training plan '{plan.Id}' session '{session.Id}'";

        if (session.Runs.Count == 0 && session.Workouts.Count == 0)
            errors.Add($"{label} must contain at least one run or workout.");

        if (!HasResolvableDate(session))
            errors.Add($"{label} must define a date, appointment start, or dated run.");

        if (session.Appointment is { StartsAt: not null, EndsAt: not null }
            && session.Appointment.EndsAt < session.Appointment.StartsAt)
        {
            errors.Add($"{label} appointment end must not be before the start.");
        }

        foreach (var run in session.Runs)
            ValidateRun(label, run, errors);

        foreach (var workout in session.Workouts)
            ValidateWorkout(label, workout, errors);

        ValidateActivities(label, "warmup", session.Warmup, errors);
        ValidateActivities(label, "drill", session.Drills, errors);
    }

    private static bool HasResolvableDate(TrainingSessionDefinition session)
    {
        return session.Date != default
               || session.Appointment?.StartsAt is not null
               || session.Runs.Any(run => GetRunDate(run) != default);
    }

    private static DateTime GetRunDate(RunningSessionDefinition run)
    {
        return run switch
        {
            IntervalSessionDefinition interval => interval.Date,
            PlannedSessionDefinition planned => planned.Date,
            _ => default
        };
    }

    private static void ValidateRun(
        string sessionLabel,
        RunningSessionDefinition run,
        List<string> errors)
    {
        switch (run)
        {
            case PlannedSessionDefinition planned:
                ValidatePlannedRun(sessionLabel, planned, errors);
                break;
            case IntervalSessionDefinition interval:
                ValidateIntervalRun(sessionLabel, interval, errors);
                break;
            default:
                errors.Add($"{sessionLabel} contains unknown run definition type '{run.GetType().Name}'.");
                break;
        }
    }

    private static void ValidatePlannedRun(
        string sessionLabel,
        PlannedSessionDefinition run,
        List<string> errors)
    {
        if (run.Sequence.Count == 0)
            errors.Add($"{sessionLabel} planned run '{run.Id}' must contain at least one segment.");

        foreach (var segment in run.Sequence)
        {
            if (segment.Distance < 0)
                errors.Add($"{sessionLabel} planned run '{run.Id}' contains a segment with negative distance.");
        }
    }

    private static void ValidateIntervalRun(
        string sessionLabel,
        IntervalSessionDefinition run,
        List<string> errors)
    {
        if (run.Distances.Count == 0)
            errors.Add($"{sessionLabel} interval run '{run.Id}' must contain at least one distance.");

        if (run.Distances.Any(distance => distance <= 0))
            errors.Add($"{sessionLabel} interval run '{run.Id}' distances must be greater than zero.");

        if (run.Recovery?.Any(distance => distance < 0) == true)
            errors.Add($"{sessionLabel} interval run '{run.Id}' recovery distances must not be negative.");

        if (run.PaceKeys.Count != 0
            && run.PaceKeys.Count != 1
            && run.PaceKeys.Count != run.Distances.Count)
        {
            errors.Add($"{sessionLabel} interval run '{run.Id}' paceKeys count must be 1 or match distances count.");
        }

        if (run.Sets < 1)
            errors.Add($"{sessionLabel} interval run '{run.Id}' sets must be greater than zero.");

        if (run.SetRecovery < 0)
            errors.Add($"{sessionLabel} interval run '{run.Id}' setRecovery must not be negative.");

        if (run.WarmupDistance < 0)
            errors.Add($"{sessionLabel} interval run '{run.Id}' warmupDistance must not be negative.");

        if (run.CooldownDistance < 0)
            errors.Add($"{sessionLabel} interval run '{run.Id}' cooldownDistance must not be negative.");
    }

    private void ValidateWorkout(
        string sessionLabel,
        WorkoutSessionDefinition workout,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(workout.WorkoutId))
        {
            errors.Add($"{sessionLabel} workoutId must not be empty.");
            return;
        }

        if (workout.Sets < 1)
            errors.Add($"{sessionLabel} workout '{workout.WorkoutId}' sets must be greater than zero.");

        if (workout.Rounds < 1)
            errors.Add($"{sessionLabel} workout '{workout.WorkoutId}' rounds must be greater than zero.");

        try
        {
            _workoutCatalog?.GetDefinition(workout.WorkoutId);
        }
        catch (KeyNotFoundException)
        {
            errors.Add($"{sessionLabel} workout '{workout.WorkoutId}' references an unknown workout.");
        }
    }

    private static void ValidateActivities(
        string sessionLabel,
        string activityKind,
        IEnumerable<TrainingSessionActivity> activities,
        List<string> errors)
    {
        foreach (var activity in activities)
        {
            if (activity is null)
            {
                errors.Add($"{sessionLabel} contains an empty {activityKind} activity.");
                continue;
            }

            var normalized = activity.Normalize();
            if (normalized.IsEmpty)
                errors.Add($"{sessionLabel} contains an empty {activityKind} activity.");

            if (normalized.DurationSeconds < 0)
                errors.Add($"{sessionLabel} {activityKind} activity duration must not be negative.");
        }
    }
}
