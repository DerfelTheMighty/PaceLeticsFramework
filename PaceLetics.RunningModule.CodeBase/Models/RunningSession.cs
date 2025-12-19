namespace PaceLetics.RunningModule.CodeBase.Models
{
    public sealed record RunningSegment(
        SegmentType Type,
        int Distance,
        string? PaceKey = null,
        TimeSpan? Duration = null
    );

    public enum SegmentType
    {
        Warmup,
        Intervall,
        Recovery,
        SetRecovery,
        Dauerlauf,
        Cooldown
    }

    public abstract class RunningSession
    {
        public string Id { get; }
        public string Name { get; }
        public DateTime Date { get; }
        public int? WarmupDistance { get; }
        public int? CooldownDistance { get; }

        protected RunningSession(
            string id,
            string name,
            DateTime date,
            int? warmupDistance,
            int? cooldownDistance)
        {
            Id = id;
            Name = name;
            Date = date;
            WarmupDistance = warmupDistance;
            CooldownDistance = cooldownDistance;
        }

        public abstract int TotalDistance { get; }
        public abstract IReadOnlyList<RunningSegment> Sequence { get; }
    }
}
