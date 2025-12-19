using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaceLetics.RunningModule.CodeBase.Models
{
    public class RunningSessionDto
    {
        public string SessionType { get; set; } = ""; // "interval" | "planned"
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

        private static readonly JsonSerializerOptions Opt = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public static IReadOnlyList<RunningSession> Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var trimmed = json.TrimStart();

            if (trimmed.StartsWith("["))
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.EnumerateArray().Select(ParseOne).ToList();
            }
            else
            {
                using var doc = JsonDocument.Parse(json);
                return new[] { ParseOne(doc.RootElement) };
            }
        }

        private static RunningSession ParseOne(JsonElement el)
        {
            var sessionType = el.GetProperty("sessionType").GetString()?.Trim().ToLowerInvariant();

            return sessionType switch
            {
                "interval" => CreateInterval(el.Deserialize<IntervalSessionDto>(Opt)!),
                "planned" => CreatePlanned(el.Deserialize<PlannedSessionDto>(Opt)!),
                _ => throw new InvalidDataException($"Unknown sessionType: {sessionType}")
            };
        }

        private static RunningSession CreateInterval(IntervalSessionDto d)
        {
            var recovery = d.Recovery ?? new List<int>();

            var paceKeys = d.PaceKeys ?? new List<string>();
            if (paceKeys.Count == 1 && d.Distances.Count > 1)
                paceKeys = Enumerable.Repeat(paceKeys[0], d.Distances.Count).ToList();

            return new IntervallSession(
                d.Id, d.Name, d.Date,
                d.Distances, recovery, paceKeys,
                d.Sets, d.SetRecovery,
                d.WarmupDistance, d.CooldownDistance
            );
        }

        private static RunningSession CreatePlanned(PlannedSessionDto d)
        {
            return new PlannedRunSession(d.Id, d.Name, d.Date,
                d.Sequence.Select(s => new RunningSegment(s.Type, s.Distance, s.PaceKey, s.Duration)).ToList()
            );
        }
    }
}
