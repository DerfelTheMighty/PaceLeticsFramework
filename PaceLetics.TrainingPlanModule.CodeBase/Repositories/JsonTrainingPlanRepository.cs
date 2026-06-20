using System.Text.Json;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;

namespace PaceLetics.TrainingPlanModule.CodeBase.Repositories;

public sealed class JsonTrainingPlanRepository : ITrainingPlanRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _directoryPath;
    private readonly IWorkoutCatalog? _workoutCatalog;

    public JsonTrainingPlanRepository(string directoryPath, IWorkoutCatalog? workoutCatalog = null)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Training plan directory path must not be empty.", nameof(directoryPath));

        _directoryPath = directoryPath;
        _workoutCatalog = workoutCatalog;
    }

    public IReadOnlyList<TrainingPlan> Load()
    {
        if (!Directory.Exists(_directoryPath))
            return Array.Empty<TrainingPlan>();

        var plans = new List<TrainingPlan>();
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

    private TrainingPlan LoadPlan(string filePath)
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
            return new TrainingPlan(
                fallbackId,
                ToReadableName(fallbackId),
                new[] { CreateRunOnlyTrainingSession(RunningSessionFactory.Create(definition)) });
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

            return new TrainingPlan(
                id,
                name,
                sessionsElement.EnumerateArray().Select(ParseTrainingSession).ToList());
        }

        throw new InvalidDataException("Unsupported training plan JSON format.");
    }

    private TrainingPlan LoadLegacyPlan(string fallbackId, JsonElement root)
    {
        var sessions = root
            .EnumerateArray()
            .Select(JsonRunningSessionRepository.ParseDefinition)
            .Select(RunningSessionFactory.Create)
            .Select(CreateRunOnlyTrainingSession)
            .ToList();

        return new TrainingPlan(fallbackId, ToReadableName(fallbackId), sessions);
    }

    private TrainingSession ParseTrainingSession(JsonElement sessionElement)
    {
        var dto = sessionElement.Deserialize<TrainingSessionDocument>(Options)
            ?? throw new InvalidDataException("Could not deserialize training session.");

        var runs = sessionElement.TryGetProperty("runs", out var runsElement)
            ? runsElement
                .EnumerateArray()
                .Select(JsonRunningSessionRepository.ParseDefinition)
                .Select(RunningSessionFactory.Create)
                .ToList()
            : new List<RunningSession>();

        var workouts = dto.Workouts.Select(ValidateWorkout).ToList();
        var primaryRun = runs.FirstOrDefault();

        var appointment = dto.Appointment ?? TrainingSessionAppointment.Empty;

        var date = dto.Date != default
            ? dto.Date
            : appointment.StartsAt?.Date
                ?? primaryRun?.Date
                ?? throw new InvalidDataException($"Training session '{dto.Id}' must define a date when it has no runs or appointment.");

        var id = !string.IsNullOrWhiteSpace(dto.Id)
            ? dto.Id
            : primaryRun?.Id
                ?? CreateSessionId(name: dto.Name, date)
                ?? throw new InvalidDataException("Training session id must not be empty.");

        var name = !string.IsNullOrWhiteSpace(dto.Name)
            ? dto.Name
            : primaryRun?.Name ?? id;

        return new TrainingSession(
            id,
            name,
            date,
            runs,
            workouts,
            dto.Warmup,
            dto.Drills,
            dto.TrainingEffect,
            appointment);
    }

    private WorkoutSessionDefinition ValidateWorkout(WorkoutSessionDefinition workout)
    {
        if (string.IsNullOrWhiteSpace(workout.WorkoutId))
            throw new InvalidDataException("Workout session workoutId must not be empty.");

        if (workout.Sets < 1)
            throw new InvalidDataException($"Workout session '{workout.WorkoutId}' sets must be greater than zero.");

        if (workout.Rounds < 1)
            throw new InvalidDataException($"Workout session '{workout.WorkoutId}' rounds must be greater than zero.");

        _workoutCatalog?.GetDefinition(workout.WorkoutId);
        return workout;
    }

    private static TrainingSession CreateRunOnlyTrainingSession(RunningSession session)
    {
        return new TrainingSession(
            session.Id,
            session.Name,
            session.Date,
            new[] { session },
            Array.Empty<WorkoutSessionDefinition>());
    }

    private static string ToReadableName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return id;

        var name = id.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static string? CreateSessionId(string name, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-');

        return $"{date:yyyy-MM-dd}-{normalizedName}";
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
