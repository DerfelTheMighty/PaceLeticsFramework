using System.Text;
using System.Text.Json;
using System.Globalization;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace Paceletics.Domain.Training
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

        /// <summary>
        /// Pace-Bereiche/Keys ("E Pace", "M Pace", "T Pace", "I Pace", "R Pace").
        /// Wenn Count == 1, wird der Eintrag auf alle Intervalle repliziert.
        /// </summary>
        public List<string> PaceKeys { get; set; } = new();

        public int Sets { get; set; } = 1;
        public int SetRecovery { get; set; } = 0;

        /// <summary>
        /// Datum, an dem diese Einheit geplant ist.
        /// </summary>
        public DateTime Date { get; set; }
    }

    public static class IntervallTrainingFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static IReadOnlyList<IntervallTraining> LoadFromJsonFile(string filePath)
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

        public static IntervallTraining CreateFromDefinition(IntervallTrainingDefinition def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.Distances == null || def.Distances.Count == 0)
                throw new ArgumentException("Distances must not be empty.", nameof(def));

            var distances = def.Distances;
            var recovery = def.Recovery ?? new List<int>();
            var paceKeys = def.PaceKeys ?? new List<string>();

            // 1 PaceKey → für alle Intervalle verwenden
            if (paceKeys.Count == 1 && distances.Count > 1)
            {
                var key = paceKeys[0];
                paceKeys = Enumerable.Repeat(key, distances.Count).ToList();
            }

            if (paceKeys.Count != distances.Count)
                throw new ArgumentException("PaceKeys.Count must match Distances.Count (or be 1).", nameof(def));

            return new IntervallTraining(
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

    /// <summary>
    /// Intervall-Training-Template, das mit einem PaceModel "scharf" gemacht werden kann.
    /// </summary>
    public sealed class IntervallTraining
    {
        public string Id { get; }
        public string Name { get; }

        /// <summary>Geplantes Datum der Einheit.</summary>
        public DateTime Date { get; }

        public IReadOnlyList<int> Distances { get; }
        public IReadOnlyList<int> Recovery { get; }
        public IReadOnlyList<string> PaceKeys => _paceKeys.AsReadOnly();

        public IReadOnlyList<TimeSpan> Paces => _paces.AsReadOnly();
        public IReadOnlyList<TimeSpan> IntervallTime => _intervalTimes.AsReadOnly();
        public IReadOnlyList<TimeSpan> LabTime => _lapTimes.AsReadOnly();

        public int Sets { get; }
        public int SetRecovery { get; }

        public PaceModel? AppliedPaceModel { get; private set; }

        private readonly List<string> _paceKeys;
        private readonly List<TimeSpan> _paces = new();
        private readonly List<TimeSpan> _intervalTimes = new();
        private readonly List<TimeSpan> _lapTimes = new();

        public IntervallTraining(
            string id,
            string name,
            DateTime date,
            IReadOnlyList<int> distances,
            IReadOnlyList<int> recovery,
            IReadOnlyList<string> paceKeys,
            int sets,
            int setRecovery)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id must not be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty.", nameof(name));

            if (distances == null) throw new ArgumentNullException(nameof(distances));
            if (recovery == null) throw new ArgumentNullException(nameof(recovery));
            if (paceKeys == null) throw new ArgumentNullException(nameof(paceKeys));

            if (distances.Count == 0)
                throw new ArgumentException("At least one interval distance is required.", nameof(distances));

            if (paceKeys.Count != distances.Count)
                throw new ArgumentException("PaceKeys.Count must match Distances.Count.", nameof(paceKeys));

            if (!(recovery.Count == distances.Count || recovery.Count == distances.Count - 1 || recovery.Count == 0))
                throw new ArgumentException(
                    "Recovery.Count must be 0, Distances.Count or Distances.Count - 1.", nameof(recovery));

            if (sets <= 0)
                throw new ArgumentOutOfRangeException(nameof(sets), "Sets must be >= 1.");
            if (setRecovery < 0)
                throw new ArgumentOutOfRangeException(nameof(setRecovery), "SetRecovery must be >= 0.");

            Id = id;
            Name = name;
            Date = date;
            Sets = sets;
            SetRecovery = setRecovery;

            Distances = distances.ToList().AsReadOnly();
            Recovery = recovery.ToList().AsReadOnly();
            _paceKeys = paceKeys.ToList();
        }

        public void ApplyPaceModel(PaceModel paceModel)
        {
            if (paceModel == null) throw new ArgumentNullException(nameof(paceModel));

            AppliedPaceModel = paceModel;
            _paces.Clear();
            _intervalTimes.Clear();
            _lapTimes.Clear();

            for (int i = 0; i < Distances.Count; i++)
            {
                var dist = Distances[i];
                var key = _paceKeys[i]; // "E Pace", "M Pace", "T Pace", "I Pace", "R Pace"

                var pace = paceModel.GetPace(key);
                _paces.Add(pace);

                var intervalSeconds = dist * pace.TotalSeconds / 1000.0;
                var intervalTime = TimeSpan.FromSeconds(Math.Round(intervalSeconds));
                _intervalTimes.Add(intervalTime);

                if (dist > 400)
                {
                    var lapSeconds = 400 * pace.TotalSeconds / 1000.0;
                    _lapTimes.Add(TimeSpan.FromSeconds(Math.Round(lapSeconds)));
                }
                else
                {
                    _lapTimes.Add(intervalTime);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Intervall Training: {Name} ({Id})");
            sb.AppendLine($"Datum: {Date:yyyy-MM-dd}");
            sb.AppendLine($"Sets: {Sets}, Erholung zwischen Sets: {SetRecovery}m");

            if (!_paces.Any())
            {
                sb.AppendLine("Hinweis: Es wurde noch kein PaceModel angewendet (ApplyPaceModel).");
                sb.AppendLine("Template:");
                sb.AppendLine("Intervall | Distanz | PaceKey   | Erholung");

                for (int i = 0; i < Distances.Count; i++)
                {
                    string recoveryText = i < Recovery.Count && Recovery.Count > 0
                        ? $"{Recovery[i]}m"
                        : "N/A";

                    sb.AppendLine(
                        $"{i + 1,8} | " +
                        $"{Distances[i],7}m | " +
                        $"{_paceKeys[i],-9} | " +
                        $"{recoveryText}");
                }

                return sb.ToString();
            }

            sb.AppendLine("Intervall | Distanz | PaceKey   | Pace       | Intervallzeit | Labzeit     | Erholung");

            for (int i = 0; i < Distances.Count; i++)
            {
                var distance = Distances[i];
                var key = _paceKeys[i];
                var pace = _paces[i].ToString(@"mm\:ss", CultureInfo.InvariantCulture);
                var interval = _intervalTimes[i].ToString(@"mm\:ss", CultureInfo.InvariantCulture);
                var lap = _lapTimes[i].ToString(@"mm\:ss", CultureInfo.InvariantCulture);

                string recoveryText = i < Recovery.Count && Recovery.Count > 0
                    ? $"{Recovery[i]}m"
                    : "N/A";

                sb.AppendLine(
                    $"{i + 1,8} | " +
                    $"{distance,7}m | " +
                    $"{key,-9} | " +
                    $"{pace,9} min/km | " +
                    $"{interval,13} | " +
                    $"{lap,10} | " +
                    $"{recoveryText}");
            }

            return sb.ToString();
        }
    }
}
