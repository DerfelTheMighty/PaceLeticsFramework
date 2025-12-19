namespace PaceLetics.RunningModule.CodeBase.Models
{
    public sealed class PlannedRunSession : RunningSession
    {
        private readonly IReadOnlyList<RunningSegment> _sequence;

        public PlannedRunSession(
            string id,
            string name,
            DateTime date,
            IReadOnlyList<RunningSegment> sequence)
            : base(id, name, date, null, null)
        {
            _sequence = sequence;
        }

        public override IReadOnlyList<RunningSegment> Sequence => _sequence;
        public override int TotalDistance => _sequence.Sum(s => s.Distance);
    }
}
