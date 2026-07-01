namespace PaceLetics.TrainingPlanModule.CodeBase.Models;

public sealed class TrainingPlanBlock
{
    public TrainingPlanBlock(
        string id,
        string name,
        IEnumerable<string> sessionIds,
        int order = 0,
        string focus = "",
        string structure = "",
        string description = "")
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name must not be empty.", nameof(name));
        if (sessionIds is null) throw new ArgumentNullException(nameof(sessionIds));

        Id = id.Trim();
        Name = name.Trim();
        Focus = focus?.Trim() ?? string.Empty;
        Structure = structure?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        Order = order;
        SessionIds = sessionIds
            .Where(sessionId => !string.IsNullOrWhiteSpace(sessionId))
            .Select(sessionId => sessionId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }

    public string Id { get; }
    public string Name { get; }
    public string Focus { get; }
    public string Structure { get; }
    public string Description { get; }
    public int Order { get; }
    public IReadOnlyList<string> SessionIds { get; }
    public bool IsEmpty => SessionIds.Count == 0;

    public bool Contains(TrainingSession session)
    {
        return session is not null
               && SessionIds.Contains(session.Id, StringComparer.OrdinalIgnoreCase);
    }
}
