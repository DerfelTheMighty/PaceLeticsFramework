using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed class TrainingSession
{
    public TrainingSession(
        string id,
        string name,
        DateTime date,
        IEnumerable<RunningSession> runs,
        IEnumerable<WorkoutSessionDefinition> workouts)
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
    }

    public string Id { get; }
    public string Name { get; }
    public DateTime Date { get; }
    public IReadOnlyList<RunningSession> Runs { get; }
    public IReadOnlyList<WorkoutSessionDefinition> Workouts { get; }
    public int TotalRunDistance => Runs.Sum(run => run.TotalDistance);
    public RunningSession? PrimaryRun => Runs.FirstOrDefault();
}
