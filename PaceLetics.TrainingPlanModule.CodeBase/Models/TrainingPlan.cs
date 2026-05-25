using System.Globalization;
using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed class TrainingPlan
{
    public TrainingPlan(string id, string name, IEnumerable<TrainingSession> sessions)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (sessions is null) throw new ArgumentNullException(nameof(sessions));

        Sessions = sessions.OrderBy(s => s.Date).ToList().AsReadOnly();
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; }
    public IReadOnlyList<TrainingSession> Sessions { get; }
    public IEnumerable<RunningSession> RunningSessions => Sessions.SelectMany(session => session.Runs);
    public DateTime? StartDate => Sessions.Count == 0 ? null : Sessions.Min(s => s.Date);
    public DateTime? EndDate => Sessions.Count == 0 ? null : Sessions.Max(s => s.Date);
    public int TotalRunDistance => Sessions.Sum(session => session.TotalRunDistance);

    public IEnumerable<IGrouping<int, TrainingSession>> ByIsoWeek()
    {
        return Sessions.GroupBy(s => ISOWeek.GetWeekOfYear(s.Date));
    }
}

public sealed class ResolvedTrainingSession
{
    public ResolvedTrainingSession(
        TrainingSession session,
        IEnumerable<ResolvedRunningSession> runs)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Runs = runs.OrderBy(run => run.Date).ToList().AsReadOnly();
    }

    public TrainingSession Session { get; }
    public IReadOnlyList<ResolvedRunningSession> Runs { get; }
    public int TotalDistance => Runs.Sum(run => run.TotalDistance);
}

public sealed class ResolvedTrainingPlan
{
    public ResolvedTrainingPlan(
        TrainingPlan plan,
        IEnumerable<ResolvedTrainingSession> sessions)
    {
        Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        Sessions = sessions.OrderBy(session => session.Session.Date).ToList().AsReadOnly();
    }

    public TrainingPlan Plan { get; }
    public IReadOnlyList<ResolvedTrainingSession> Sessions { get; }
    public int TotalDistance => Sessions.Sum(s => s.TotalDistance);
}
