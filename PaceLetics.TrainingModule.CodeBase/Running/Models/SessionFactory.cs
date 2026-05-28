namespace PaceLetics.TrainingModule.CodeBase.Running.Models
{
    public class RunningSessionDto
    {
        public string SessionType { get; set; } = "";
    }

    public sealed class IntervalSessionDto : RunningSessionDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public int? WarmupDistance { get; set; }
        public int? CooldownDistance { get; set; }
        public List<int> Distances { get; set; } = new();
        public List<int>? Recovery { get; set; } = new();
        public List<string> PaceKeys { get; set; } = new();
        public int Sets { get; set; } = 1;
        public int SetRecovery { get; set; } = 0;
    }

    public sealed class PlannedSessionDto : RunningSessionDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public List<RunningSegmentDto> Sequence { get; set; } = new();
    }

    public sealed class RunningSegmentDto
    {
        public SegmentType Type { get; set; }
        public int Distance { get; set; }
        public string? PaceKey { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    public static class RunningSessionFactory
    {
        public static RunningSession Create(RunningSessionDto definition)
        {
            return definition switch
            {
                IntervalSessionDto interval => CreateInterval(interval),
                PlannedSessionDto planned => CreatePlanned(planned),
                _ => throw new InvalidDataException($"Unknown session definition type: {definition.GetType().Name}")
            };
        }

        public static IReadOnlyList<RunningSession> Create(IEnumerable<RunningSessionDto> definitions)
        {
            return definitions.Select(Create).ToList();
        }

        private static RunningSession CreateInterval(IntervalSessionDto definition)
        {
            var recovery = definition.Recovery ?? new List<int>();

            var paceKeys = definition.PaceKeys ?? new List<string>();
            if (paceKeys.Count == 1 && definition.Distances.Count > 1)
                paceKeys = Enumerable.Repeat(paceKeys[0], definition.Distances.Count).ToList();

            return new IntervallSession(
                definition.Id,
                definition.Name,
                definition.Date,
                definition.Distances,
                recovery,
                paceKeys,
                definition.Sets,
                definition.SetRecovery,
                definition.WarmupDistance,
                definition.CooldownDistance);
        }

        private static RunningSession CreatePlanned(PlannedSessionDto definition)
        {
            return new PlannedRunSession(
                definition.Id,
                definition.Name,
                definition.Date,
                definition.Sequence
                    .Select(s => new RunningSegment(s.Type, s.Distance, s.PaceKey, s.Duration))
                    .ToList());
        }
    }
}
