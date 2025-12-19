using PaceLetics.CoreModule.Infrastructure.Models; 
namespace PaceLetics.RunningModule.CodeBase.Models
{
    public sealed record ResolvedRunningSegment(
        RunningSegment Segment,
        TimeSpan? Pace,
        TimeSpan? SegmentTime,
        TimeSpan? LapTime
    );

    public sealed record ResolvedRunningSession(
        string Id,
        string Name,
        DateTime Date,
        int TotalDistance,
        IReadOnlyList<ResolvedRunningSegment> Segments
    );

    public static class RunningSessionResolver
    {
        /// <summary>
        /// Resolves a RunningSession template into a computed session using the given PaceModel.
        /// Domain objects remain immutable; output is a separate computed structure.
        /// </summary>
        public static ResolvedRunningSession Resolve(RunningSession session, PaceModel paceModel)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (paceModel is null) throw new ArgumentNullException(nameof(paceModel));

            Validate(session);

            var resolved = session.Sequence.Select(seg =>
            {
                // pace only if key exists
                TimeSpan? pace = seg.PaceKey is null ? null : paceModel.GetPace(seg.PaceKey);

                // duration: prefer explicit Duration, else compute from distance+pace
                TimeSpan? segmentTime =
                    seg.Duration ??
                    (pace is null || seg.Distance <= 0
                        ? null
                        : TimeSpan.FromSeconds(Math.Round(seg.Distance * pace.Value.TotalSeconds / 1000.0)));

                // lap time: only for running segments where splits make sense
                TimeSpan? lapTime = null;
                if (pace is not null && seg.Distance > 0 && ShouldHaveLap(seg.Type))
                {
                    var lapDist = seg.Distance > 400 ? 400 : seg.Distance;
                    lapTime = TimeSpan.FromSeconds(Math.Round(lapDist * pace.Value.TotalSeconds / 1000.0));
                }

                return new ResolvedRunningSegment(seg, pace, segmentTime, lapTime);
            }).ToList();

            return new ResolvedRunningSession(
                session.Id,
                session.Name,
                session.Date,
                session.TotalDistance,
                resolved
            );
        }

        private static bool ShouldHaveLap(SegmentType type) =>
            type is SegmentType.Intervall or SegmentType.Dauerlauf;

        private static void Validate(RunningSession session)
        {
            if (string.IsNullOrWhiteSpace(session.Id))
                throw new ArgumentException("Session.Id must not be empty.", nameof(session));
            if (string.IsNullOrWhiteSpace(session.Name))
                throw new ArgumentException("Session.Name must not be empty.", nameof(session));

            if (session.Sequence is null || session.Sequence.Count == 0)
                throw new ArgumentException("Session.Sequence must not be empty.", nameof(session));

            for (int i = 0; i < session.Sequence.Count; i++)
            {
                var seg = session.Sequence[i];

                // need at least distance or duration
                var hasDistance = seg.Distance > 0;
                var hasDuration = seg.Duration.HasValue && seg.Duration.Value > TimeSpan.Zero;

                if (!hasDistance && !hasDuration)
                    throw new ArgumentException($"Segment[{i}] must have Distance > 0 or Duration > 0.");

                // if pace key is set but neither distance nor duration can be computed -> still ok if duration given
                if (seg.PaceKey is not null && !hasDistance && !hasDuration)
                    throw new ArgumentException($"Segment[{i}] has PaceKey but no Distance/Duration.");
            }
        }
    }
}
