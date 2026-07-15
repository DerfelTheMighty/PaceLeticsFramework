using System.Text.Json;
using System.Text.Json.Serialization;
using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Repositories;

public sealed class JsonTrainingPlanRepository : ITrainingPlanRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly string _directoryPath;

    public JsonTrainingPlanRepository(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Training plan directory path must not be empty.", nameof(directoryPath));

        _directoryPath = directoryPath;
    }

    public IReadOnlyList<TrainingPlanDefinition> Load()
    {
        if (!Directory.Exists(_directoryPath))
            return Array.Empty<TrainingPlanDefinition>();

        var plans = new List<TrainingPlanDefinition>();
        foreach (var file in Directory.EnumerateFiles(_directoryPath, "*.json"))
        {
            try
            {
                plans.Add(LoadPlan(file));
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException or ArgumentException or KeyNotFoundException)
            {
                throw new InvalidDataException($"Training plan file '{file}' could not be loaded.", ex);
            }
        }

        return plans;
    }

    public void Save(TrainingPlanDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Directory.CreateDirectory(_directoryPath);

        var filePath = ResolvePlanFilePath(definition);
        File.WriteAllText(filePath, Serialize(definition));
    }

    public static string Serialize(TrainingPlanDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            WriteTrainingPlan(writer, definition);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    public static TrainingPlanDefinition Deserialize(string json, string fallbackId)
    {
        using var document = JsonDocument.Parse(json);
        return ParsePlan(document.RootElement, json, fallbackId);
    }

    private TrainingPlanDefinition LoadPlan(string filePath)
    {
        var json = File.ReadAllText(filePath);
        using var document = JsonDocument.Parse(json);
        var fallbackId = Path.GetFileNameWithoutExtension(filePath) ?? Guid.NewGuid().ToString();
        return ParsePlan(document.RootElement, json, fallbackId);
    }

    private static TrainingPlanDefinition ParsePlan(JsonElement root, string json, string fallbackId)
    {

        if (root.ValueKind == JsonValueKind.Array)
            return LoadLegacyPlan(fallbackId, root);

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("sessionType", out _))
        {
            var definition = JsonRunningSessionRepository.ParseDefinition(root);
            return new TrainingPlanDefinition
            {
                Id = fallbackId,
                Name = ToReadableName(fallbackId),
                Sessions = [CreateRunOnlyTrainingSession(definition)]
            };
        }

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("sessions", out var sessionsElement))
        {
            var planDocument = JsonSerializer.Deserialize<TrainingPlanDocument>(json, Options)
                ?? throw new InvalidDataException("Could not deserialize training plan document.");

            var id = string.IsNullOrWhiteSpace(planDocument.Id) ? fallbackId : planDocument.Id;
            var name = string.IsNullOrWhiteSpace(planDocument.Name)
                ? ToReadableName(id)
                : planDocument.Name;

            return new TrainingPlanDefinition
            {
                SchemaVersion = planDocument.SchemaVersion,
                Id = id,
                Name = name,
                Blocks = planDocument.Blocks,
                Sessions = sessionsElement.EnumerateArray().Select(ParseTrainingSession).ToList()
            };
        }

        throw new InvalidDataException("Unsupported training plan JSON format.");
    }

    private static TrainingPlanDefinition LoadLegacyPlan(string fallbackId, JsonElement root)
    {
        var sessions = root
            .EnumerateArray()
            .Select(JsonRunningSessionRepository.ParseDefinition)
            .Select(CreateRunOnlyTrainingSession)
            .ToList();

        return new TrainingPlanDefinition
        {
            Id = fallbackId,
            Name = ToReadableName(fallbackId),
            Sessions = sessions
        };
    }

    private static TrainingSessionDefinition ParseTrainingSession(JsonElement sessionElement)
    {
        var dto = sessionElement.Deserialize<TrainingSessionDocument>(Options)
            ?? throw new InvalidDataException("Could not deserialize training session.");

        var runs = sessionElement.TryGetProperty("runs", out var runsElement)
            ? runsElement
                .EnumerateArray()
                .Select(JsonRunningSessionRepository.ParseDefinition)
                .ToList()
            : [];

        return new TrainingSessionDefinition
        {
            Id = dto.Id,
            Name = dto.Name,
            Date = dto.Date,
            Runs = runs,
            Workouts = dto.Workouts,
            Warmup = dto.Warmup,
            Drills = dto.Drills,
            TrainingEffect = dto.TrainingEffect,
            Appointment = dto.Appointment
        };
    }

    private static TrainingSessionDefinition CreateRunOnlyTrainingSession(RunningSessionDefinition session)
    {
        return new TrainingSessionDefinition
        {
            Runs = [session]
        };
    }

    private string ResolvePlanFilePath(TrainingPlanDefinition definition)
    {
        foreach (var file in Directory.EnumerateFiles(_directoryPath, "*.json"))
        {
            try
            {
                var plan = LoadPlan(file);
                if (string.Equals(plan.Id, definition.Id, StringComparison.OrdinalIgnoreCase))
                    return file;
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException or ArgumentException or KeyNotFoundException)
            {
                // Ignore unrelated broken files while resolving the target file name.
            }
        }

        return Path.Combine(_directoryPath, $"{SanitizeFileName(definition.Id)}.json");
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string(value
            .Select(character => invalidChars.Contains(character) ? '-' : character)
            .ToArray()).Trim();

        return string.IsNullOrWhiteSpace(sanitized)
            ? Guid.NewGuid().ToString("N")
            : sanitized;
    }

    private static void WriteTrainingPlan(Utf8JsonWriter writer, TrainingPlanDefinition definition)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", Math.Max(definition.SchemaVersion, 2));
        writer.WriteString("id", definition.Id);
        writer.WriteString("name", definition.Name);

        if (definition.Blocks.Count > 0)
        {
            writer.WritePropertyName("blocks");
            JsonSerializer.Serialize(writer, definition.Blocks, Options);
        }

        writer.WritePropertyName("sessions");
        writer.WriteStartArray();
        foreach (var session in definition.Sessions)
            WriteTrainingSession(writer, session);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteTrainingSession(Utf8JsonWriter writer, TrainingSessionDefinition session)
    {
        writer.WriteStartObject();
        WriteStringIfNotEmpty(writer, "id", session.Id);
        WriteStringIfNotEmpty(writer, "name", session.Name);

        if (session.Date != default)
            writer.WriteString("date", session.Date.ToString("yyyy-MM-dd"));

        if (session.Appointment is not null && !session.Appointment.IsEmpty)
        {
            writer.WritePropertyName("appointment");
            JsonSerializer.Serialize(writer, session.Appointment, Options);
        }

        if (session.TrainingEffect is not null && !session.TrainingEffect.IsEmpty)
        {
            writer.WritePropertyName("trainingEffect");
            JsonSerializer.Serialize(writer, session.TrainingEffect, Options);
        }

        if (session.Warmup.Count > 0)
        {
            writer.WritePropertyName("warmup");
            JsonSerializer.Serialize(writer, session.Warmup, Options);
        }

        if (session.Drills.Count > 0)
        {
            writer.WritePropertyName("drills");
            JsonSerializer.Serialize(writer, session.Drills, Options);
        }

        writer.WritePropertyName("runs");
        writer.WriteStartArray();
        foreach (var run in session.Runs)
            WriteRun(writer, run);
        writer.WriteEndArray();

        writer.WritePropertyName("workouts");
        JsonSerializer.Serialize(writer, session.Workouts, Options);
        writer.WriteEndObject();
    }

    private static void WriteRun(Utf8JsonWriter writer, RunningSessionDefinition run)
    {
        switch (run)
        {
            case PlannedSessionDefinition planned:
                WritePlannedRun(writer, planned);
                break;
            case IntervalSessionDefinition interval:
                WriteIntervalRun(writer, interval);
                break;
            default:
                throw new InvalidDataException($"Unsupported run definition type '{run.GetType().Name}'.");
        }
    }

    private static void WritePlannedRun(Utf8JsonWriter writer, PlannedSessionDefinition run)
    {
        writer.WriteStartObject();
        writer.WriteString("sessionType", "planned");
        WriteStringIfNotEmpty(writer, "id", run.Id);
        WriteStringIfNotEmpty(writer, "name", run.Name);
        if (run.Date != default)
            writer.WriteString("date", run.Date.ToString("yyyy-MM-dd"));

        writer.WritePropertyName("sequence");
        JsonSerializer.Serialize(writer, run.Sequence, Options);
        writer.WriteEndObject();
    }

    private static void WriteIntervalRun(Utf8JsonWriter writer, IntervalSessionDefinition run)
    {
        writer.WriteStartObject();
        writer.WriteString("sessionType", "interval");
        WriteStringIfNotEmpty(writer, "id", run.Id);
        WriteStringIfNotEmpty(writer, "name", run.Name);
        if (run.Date != default)
            writer.WriteString("date", run.Date.ToString("yyyy-MM-dd"));

        WriteNumberIfSet(writer, "warmupDistance", run.WarmupDistance);
        WriteNumberIfSet(writer, "cooldownDistance", run.CooldownDistance);

        writer.WritePropertyName("distances");
        JsonSerializer.Serialize(writer, run.Distances, Options);

        if (run.Recovery is not null)
        {
            writer.WritePropertyName("recovery");
            JsonSerializer.Serialize(writer, run.Recovery, Options);
        }

        writer.WritePropertyName("paceKeys");
        JsonSerializer.Serialize(writer, run.PaceKeys, Options);
        writer.WriteNumber("sets", run.Sets);
        writer.WriteNumber("setRecovery", run.SetRecovery);
        writer.WriteEndObject();
    }

    private static void WriteStringIfNotEmpty(Utf8JsonWriter writer, string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            writer.WriteString(name, value);
    }

    private static void WriteNumberIfSet(Utf8JsonWriter writer, string name, int? value)
    {
        if (value is not null)
            writer.WriteNumber(name, value.Value);
    }

    private static string ToReadableName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return id;

        var name = id.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private sealed class TrainingPlanDocument
    {
        public int SchemaVersion { get; set; }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<TrainingPlanBlockDefinition> Blocks { get; set; } = new();
    }

    private sealed class TrainingSessionDocument
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public List<WorkoutSessionDefinition> Workouts { get; set; } = new();
        public List<TrainingSessionActivity> Warmup { get; set; } = new();
        public List<TrainingSessionActivity> Drills { get; set; } = new();
        public TrainingEffect? TrainingEffect { get; set; }
        public TrainingSessionAppointment? Appointment { get; set; }
    }
}
