using System.Text.Json;
using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.RunningModule.CodeBase.Repositories;

public sealed class JsonTrainingPlanRepository : ITrainingPlanRepository
{
    private readonly string _directoryPath;

    public JsonTrainingPlanRepository(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Training plan directory path must not be empty.", nameof(directoryPath));

        _directoryPath = directoryPath;
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
                var definitions = new JsonRunningSessionRepository(file).Load();
                var sessions = RunningSessionFactory.Create(definitions);
                var id = Path.GetFileNameWithoutExtension(file) ?? Guid.NewGuid().ToString();
                plans.Add(new TrainingPlan(id, ToReadableName(id), sessions));
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException or ArgumentException)
            {
                throw new InvalidDataException($"Training plan file '{file}' could not be loaded.", ex);
            }
        }

        return plans;
    }

    private static string ToReadableName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return id;

        var name = id.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
