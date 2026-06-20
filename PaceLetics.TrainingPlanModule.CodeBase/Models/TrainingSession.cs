using PaceLetics.TrainingModule.CodeBase.Running.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed class TrainingSession
{
    public TrainingSession(
        string id,
        string name,
        DateTime date,
        IEnumerable<RunningSession> runs,
        IEnumerable<WorkoutSessionDefinition> workouts)
        : this(
            id,
            name,
            date,
            runs,
            workouts,
            Array.Empty<TrainingSessionActivity>(),
            Array.Empty<TrainingSessionActivity>(),
            TrainingEffect.Empty,
            TrainingSessionAppointment.Empty)
    {
    }

    public TrainingSession(
        string id,
        string name,
        DateTime date,
        IEnumerable<RunningSession> runs,
        IEnumerable<WorkoutSessionDefinition> workouts,
        IEnumerable<TrainingSessionActivity>? warmup,
        IEnumerable<TrainingSessionActivity>? drills,
        TrainingEffect? trainingEffect,
        TrainingSessionAppointment? appointment)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (runs is null) throw new ArgumentNullException(nameof(runs));
        if (workouts is null) throw new ArgumentNullException(nameof(workouts));

        Runs = runs.OrderBy(run => run.Date).ToList().AsReadOnly();
        Workouts = workouts.ToList().AsReadOnly();

        if (Runs.Count == 0 && Workouts.Count == 0)
            throw new ArgumentException("A training session must contain at least one run or workout.");

        Id = id;
        Name = name;
        Date = date;
        Warmup = NormalizeActivities(warmup, nameof(warmup));
        Drills = NormalizeActivities(drills, nameof(drills));
        TrainingEffect = (trainingEffect ?? TrainingEffect.Empty).Normalize();
        Appointment = (appointment ?? TrainingSessionAppointment.Empty).Normalize();

        if (Appointment.StartsAt is not null
            && Appointment.EndsAt is not null
            && Appointment.EndsAt < Appointment.StartsAt)
        {
            throw new ArgumentException("Appointment end must not be before the start.", nameof(appointment));
        }
    }

    public string Id { get; }
    public string Name { get; }
    public DateTime Date { get; }
    public IReadOnlyList<RunningSession> Runs { get; }
    public IReadOnlyList<WorkoutSessionDefinition> Workouts { get; }
    public IReadOnlyList<TrainingSessionActivity> Warmup { get; }
    public IReadOnlyList<TrainingSessionActivity> Drills { get; }
    public TrainingEffect TrainingEffect { get; }
    public TrainingSessionAppointment Appointment { get; }
    public int TotalRunDistance => Runs.Sum(run => run.TotalDistance);
    public RunningSession? PrimaryRun => Runs.FirstOrDefault();
    public bool HasPreparation => Warmup.Count > 0 || Drills.Count > 0;
    public bool HasTrainingEffect => !TrainingEffect.IsEmpty;
    public bool HasAppointment => !Appointment.IsEmpty;

    private static IReadOnlyList<TrainingSessionActivity> NormalizeActivities(
        IEnumerable<TrainingSessionActivity>? activities,
        string parameterName)
    {
        var normalized = new List<TrainingSessionActivity>();

        foreach (var activity in activities ?? Enumerable.Empty<TrainingSessionActivity>())
        {
            if (activity is null)
                throw new ArgumentException("Training session activity must not be null.", parameterName);

            var normalizedActivity = activity.Normalize();
            if (normalizedActivity.IsEmpty)
                throw new ArgumentException("Training session activity must not be empty.", parameterName);

            if (normalizedActivity.DurationSeconds < 0)
                throw new ArgumentException("Training session activity duration must not be negative.", parameterName);

            normalized.Add(normalizedActivity);
        }

        return normalized.AsReadOnly();
    }
}
