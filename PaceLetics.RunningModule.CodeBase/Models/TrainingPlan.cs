
using System.Globalization;


namespace PaceLetics.RunningModule.CodeBase.Models
{
    public sealed class TrainingPlan
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<RunningSession> Sessions { get; }

        public TrainingPlan(string id, string name, IEnumerable<RunningSession> sessions)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id must not be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
            if (sessions is null) throw new ArgumentNullException(nameof(sessions));

            Sessions = sessions.OrderBy(s => s.Date).ToList().AsReadOnly();
            Id = id;
            Name = name;
        }

        public DateTime? StartDate => Sessions.Count == 0 ? null : Sessions.Min(s => s.Date);
        public DateTime? EndDate => Sessions.Count == 0 ? null : Sessions.Max(s => s.Date);

        public IEnumerable<IGrouping<int, RunningSession>> ByIsoWeek()
        {
            return Sessions.GroupBy(s => ISOWeek.GetWeekOfYear(s.Date));
        }
    }


    public sealed class ResolvedTrainingPlan
    {
        public TrainingPlan Plan { get; }
        public IReadOnlyList<ResolvedRunningSession> Sessions { get; }

        public ResolvedTrainingPlan(TrainingPlan plan, IEnumerable<ResolvedRunningSession> sessions)
        {
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
            Sessions = sessions.OrderBy(s => s.Date).ToList().AsReadOnly();
        }

        public int TotalDistance => Sessions.Sum(s => s.TotalDistance);
    }

}
