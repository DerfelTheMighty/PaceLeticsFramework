using System.Text.Json;
namespace PaceLetics.RunningModule.CodeBase.Models
{
    /// <summary>
    /// JSON-Definition eines Intervalltrainings (ohne konkrete Paces).
    /// </summary>
    public sealed class IntervallTrainingDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<int> Distances { get; set; } = new();
        public List<int>? Recovery { get; set; } = new();
        public List<string> PaceKeys { get; set; } = new();
        public int Sets { get; set; } = 1;
        public int SetRecovery { get; set; } = 0;
        public DateTime Date { get; set; }
    }

    public static class IntervallSessionFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static IReadOnlyList<IntervallSession> LoadFromJsonFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("JSON file not found.", filePath);

            var json = File.ReadAllText(filePath);
            var trimmed = json.TrimStart();

            if (trimmed.StartsWith("["))
            {
                var defs = JsonSerializer.Deserialize<List<IntervallTrainingDefinition>>(json, JsonOptions)
                           ?? throw new InvalidDataException("Could not deserialize interval definitions array.");

                return defs.Select(CreateFromDefinition).ToList();
            }
            else
            {
                var def = JsonSerializer.Deserialize<IntervallTrainingDefinition>(json, JsonOptions)
                          ?? throw new InvalidDataException("Could not deserialize interval definition object.");

                return new[] { CreateFromDefinition(def) };
            }
        }

        public static IntervallSession CreateFromDefinition(IntervallTrainingDefinition def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Distances == null || def.Distances.Count == 0)
                throw new ArgumentException("Distances must not be empty.", nameof(def));

            var distances = def.Distances;
            var recovery = def.Recovery ?? new List<int>();
            var paceKeys = def.PaceKeys ?? new List<string>();

            if (paceKeys.Count == 1 && distances.Count > 1)
            {
                var key = paceKeys[0];
                paceKeys = Enumerable.Repeat(key, distances.Count).ToList();
            }

            if (paceKeys.Count != distances.Count)
                throw new ArgumentException("PaceKeys.Count must match Distances.Count (or be 1).", nameof(def));

            return new IntervallSession(
                def.Id,
                def.Name,
                def.Date,
                distances,
                recovery,
                paceKeys,
                def.Sets,
                def.SetRecovery
            );
        }
    }

    public sealed class IntervallSession : RunningSession
    {
        public IReadOnlyList<int> Distances { get; }
        public IReadOnlyList<int> Recovery { get; }
        public IReadOnlyList<string> PaceKeys { get; }

        public int Sets { get; }
        public int SetRecovery { get; }

        private readonly IReadOnlyList<RunningSegment> _sequence;

        public IntervallSession(
            string id,
            string name,
            DateTime date,
            IReadOnlyList<int> distances,
            IReadOnlyList<int> recovery,
            IReadOnlyList<string> paceKeys,
            int sets,
            int setRecovery,
            int? warmupDistance = null,
            int? cooldownDistance = null)
            : base(id, name, date, warmupDistance, cooldownDistance)
        {
            if (distances is null) throw new ArgumentNullException(nameof(distances));
            if (recovery is null) throw new ArgumentNullException(nameof(recovery));
            if (paceKeys is null) throw new ArgumentNullException(nameof(paceKeys));
            if (distances.Count == 0) throw new ArgumentException("Distances must not be empty.", nameof(distances));
            if (sets <= 0) throw new ArgumentOutOfRangeException(nameof(sets), "Sets must be >= 1.");
            if (setRecovery < 0) throw new ArgumentOutOfRangeException(nameof(setRecovery), "SetRecovery must be >= 0.");

            if (!(recovery.Count == 0 || recovery.Count == distances.Count || recovery.Count == distances.Count - 1))
                throw new ArgumentException("Recovery.Count must be 0, Distances.Count or Distances.Count - 1.", nameof(recovery));

            var pk = paceKeys.Count == 1
                ? Enumerable.Repeat(paceKeys[0], distances.Count).ToList()
                : paceKeys.ToList();

            if (pk.Count != distances.Count)
                throw new ArgumentException("PaceKeys.Count must match Distances.Count (or be 1).", nameof(paceKeys));

            Distances = distances.ToList().AsReadOnly();
            Recovery = recovery.ToList().AsReadOnly();
            PaceKeys = pk.AsReadOnly();

            Sets = sets;
            SetRecovery = setRecovery;

            _sequence = BuildSequence().AsReadOnly();
        }

        public override IReadOnlyList<RunningSegment> Sequence => _sequence;

        public override int TotalDistance => _sequence.Sum(s => s.Distance);

        private List<RunningSegment> BuildSequence()
        {
            var segments = new List<RunningSegment>();

            if (WarmupDistance is int wu && wu > 0)
                segments.Add(new(SegmentType.Warmup, wu, "E Pace"));

            for (int s = 0; s < Sets; s++)
            {
                for (int i = 0; i < Distances.Count; i++)
                {
                    segments.Add(new(SegmentType.Intervall, Distances[i], PaceKeys[i]));

                    if (i < Recovery.Count)
                        segments.Add(new(SegmentType.Recovery, Recovery[i], "E Pace"));
                }

                if (s < Sets - 1 && SetRecovery > 0)
                    segments.Add(new(SegmentType.SetRecovery, SetRecovery, "E Pace"));
            }

            if (CooldownDistance is int cd && cd > 0)
                segments.Add(new(SegmentType.Cooldown, cd, "E Pace"));

            return segments;
        }
    }
}
