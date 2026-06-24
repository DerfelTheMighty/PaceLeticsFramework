using System.Text.Json;
using System.Text.Json.Serialization;
using PaceLetics.TrainingModule.CodeBase.Running.Definitions;

namespace PaceLetics.TrainingModule.CodeBase.Running.Repositories;

public sealed class JsonRunningSessionRepository : IRunningSessionRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly string _filePath;

    public JsonRunningSessionRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Session file path must not be empty.", nameof(filePath));

        _filePath = filePath;
    }

    public IReadOnlyList<RunningSessionDefinition> Load()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("Running session file was not found.", _filePath);

        var json = File.ReadAllText(_filePath);
        var trimmed = json.TrimStart();

        using var document = JsonDocument.Parse(json);
        return trimmed.StartsWith("[")
            ? document.RootElement.EnumerateArray().Select(ParseDefinition).ToList()
            : new[] { ParseDefinition(document.RootElement) };
    }

    public static RunningSessionDefinition ParseDefinition(JsonElement element)
    {
        var sessionType = element.GetProperty("sessionType").GetString()?.Trim().ToLowerInvariant();

        return sessionType switch
        {
            "interval" => element.Deserialize<IntervalSessionDefinition>(Options)
                ?? throw new InvalidDataException("Could not deserialize interval session."),
            "planned" => element.Deserialize<PlannedSessionDefinition>(Options)
                ?? throw new InvalidDataException("Could not deserialize planned session."),
            _ => throw new InvalidDataException($"Unknown sessionType: {sessionType}")
        };
    }
}
