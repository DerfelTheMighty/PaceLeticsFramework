using System.Text.Json;
using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Repositories;

public sealed class JsonTrainingPlanRepository : ITrainingPlanRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
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

    private TrainingPlanDefinition LoadPlan(string filePath)
    {
        var json = File.ReadAllText(filePath);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var fallbackId = Path.GetFileNameWithoutExtension(filePath) ?? Guid.NewGuid().ToString();

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
                Sessions = sessionsElement.EnumerateArray().Select(ParseTrainingSession).ToList()
            };
        }

        throw new InvalidDataException("Unsupported training plan JSON format.");
    }

    private TrainingPlanDefinition LoadLegacyPlan(string fallbackId, JsonElement root)
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

    private TrainingSessionDefinition ParseTrainingSession(JsonElement sessionElement)
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
