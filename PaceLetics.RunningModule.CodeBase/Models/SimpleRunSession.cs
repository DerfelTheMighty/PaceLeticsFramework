

namespace PaceLetics.RunningModule.CodeBase.Models
{
    public sealed class SimpleRunSession : RunningSession
    {
        private readonly IReadOnlyList<RunningSegment> _sequence;

        public SimpleRunSession(
            string id,
            string name,
            DateTime date,
            int distance,
            string paceKey,
            int? warmupDistance = null,
            int? cooldownDistance = null)
            : base(id, name, date, warmupDistance, cooldownDistance)
        {
            _sequence = Build(distance, paceKey).AsReadOnly();
        }

        public override IReadOnlyList<RunningSegment> Sequence => _sequence;
        public override int TotalDistance => _sequence.Sum(s => s.Distance);

        private List<RunningSegment> Build(int dist, string pace)
        {
            var list = new List<RunningSegment>();

            if (WarmupDistance is int wu && wu > 0)
                list.Add(new(SegmentType.Warmup, wu, "E Pace"));

            list.Add(new(SegmentType.Dauerlauf, dist, pace));

            if (CooldownDistance is int cd && cd > 0)
                list.Add(new(SegmentType.Cooldown, cd, "E Pace"));

            return list;
        }
    }
}