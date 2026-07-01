using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Interfaces;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;
using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Web.Services;

public sealed class TrainingPlanService : ITrainingPlanService
{
    private readonly ITrainingPlanRepository _trainingPlanRepository;
    private readonly ITrainingPlanFactory _trainingPlanFactory;
    private readonly IRunningSessionRepository _legacyRunningSessionRepository;
    private readonly IRunningSessionFactory _runningSessionFactory;
    private readonly ICourseService _courseService;

    public TrainingPlanService(
        ITrainingPlanRepository trainingPlanRepository,
        ITrainingPlanFactory trainingPlanFactory,
        IRunningSessionRepository legacyRunningSessionRepository,
        IRunningSessionFactory runningSessionFactory,
        ICourseService courseService)
    {
        _trainingPlanRepository = trainingPlanRepository;
        _trainingPlanFactory = trainingPlanFactory;
        _legacyRunningSessionRepository = legacyRunningSessionRepository;
        _runningSessionFactory = runningSessionFactory;
        _courseService = courseService;
    }

    public IReadOnlyList<TrainingPlan> LoadTrainingPlans()
    {
        var plans = _trainingPlanFactory.Create(_trainingPlanRepository.Load()).ToList();

        if (plans.Any())
            return plans;

        var sessions = LoadLegacySessions();
        if (sessions.Count == 0)
            return Array.Empty<TrainingPlan>();

        const string id = "intervalls";
        return new[] { new TrainingPlan(
            id,
            ToReadableName(id),
            sessions.Select(session => new TrainingSession(
                session.Id,
                session.Name,
                session.Date,
                new[] { session },
                Array.Empty<WorkoutSessionDefinition>()))) };
    }

    public async Task<IReadOnlyList<TrainingPlan>> LoadTrainingPlansForUserAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<TrainingPlan>();

        var visiblePlanIds = await _courseService.GetPublishedTrainingPlanIdsForAthleteAsync(userId);
        if (visiblePlanIds.Count == 0)
            return Array.Empty<TrainingPlan>();

        var visiblePlanIdSet = visiblePlanIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return LoadTrainingPlans()
            .Where(plan => visiblePlanIdSet.Contains(plan.Id))
            .ToList();
    }

    public IReadOnlyList<RunningSession> LoadLegacySessions()
    {
        try
        {
            var definitions = _legacyRunningSessionRepository.Load();
            return _runningSessionFactory.Create(definitions);
        }
        catch (FileNotFoundException)
        {
            return Array.Empty<RunningSession>();
        }
    }

    public IReadOnlyList<TrainingPlanDefinition> LoadTrainingPlanDefinitions()
    {
        return _trainingPlanRepository.Load();
    }

    public TrainingPlan CreateTrainingPlan(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Training plan name must not be empty.", nameof(name));

        var definitions = _trainingPlanRepository.Load();
        var definition = new TrainingPlanDefinition
        {
            SchemaVersion = 2,
            Id = CreateUniqueId(definitions.Select(plan => plan.Id), name),
            Name = name.Trim()
        };

        return SaveAndCreate(definition);
    }

    public TrainingPlan UpdateTrainingPlan(string planId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Training plan name must not be empty.", nameof(name));

        var definition = GetDefinition(planId);
        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        definition.Name = name.Trim();

        return SaveAndCreate(definition);
    }

    public TrainingPlan AddTrainingSession(string planId, TrainingSessionDefinition session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var definition = GetDefinition(planId);
        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);

        var normalized = NormalizeSession(
            session,
            CreateUniqueId(definition.Sessions.Select(existing => existing.Id), ResolveSessionName(session)));

        definition.Sessions.Add(normalized);
        return SaveAndCreate(definition);
    }

    public TrainingPlan UpdateTrainingSession(string planId, string sessionId, TrainingSessionDefinition session)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Training session id must not be empty.", nameof(sessionId));
        ArgumentNullException.ThrowIfNull(session);

        var definition = GetDefinition(planId);
        var index = definition.Sessions.FindIndex(existing =>
            string.Equals(existing.Id, sessionId, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            throw new KeyNotFoundException($"Training session '{sessionId}' was not found.");

        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        definition.Sessions[index] = NormalizeSession(session, sessionId);

        return SaveAndCreate(definition);
    }

    public TrainingPlan RemoveTrainingSession(string planId, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Training session id must not be empty.", nameof(sessionId));

        var definition = GetDefinition(planId);
        var removed = definition.Sessions.RemoveAll(session =>
            string.Equals(session.Id, sessionId, StringComparison.OrdinalIgnoreCase));

        if (removed == 0)
            throw new KeyNotFoundException($"Training session '{sessionId}' was not found.");

        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        foreach (var block in definition.Blocks)
        {
            block.SessionIds = block.SessionIds
                .Where(id => !string.Equals(id, sessionId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        definition.Blocks = definition.Blocks
            .Where(block => block.SessionIds.Count > 0)
            .ToList();

        return SaveAndCreate(definition);
    }

    public TrainingPlan AddTrainingPlanBlock(string planId, TrainingPlanBlockDefinition block)
    {
        ArgumentNullException.ThrowIfNull(block);

        var definition = GetDefinition(planId);
        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        definition.Blocks.Add(NormalizeBlock(
            block,
            CreateUniqueId(definition.Blocks.Select(existing => existing.Id), block.Name),
            NextBlockOrder(definition)));

        return SaveAndCreate(definition);
    }

    public TrainingPlan UpdateTrainingPlanBlock(string planId, string blockId, TrainingPlanBlockDefinition block)
    {
        if (string.IsNullOrWhiteSpace(blockId))
            throw new ArgumentException("Training plan block id must not be empty.", nameof(blockId));
        ArgumentNullException.ThrowIfNull(block);

        var definition = GetDefinition(planId);
        var index = definition.Blocks.FindIndex(existing =>
            string.Equals(existing.Id, blockId, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            throw new KeyNotFoundException($"Training plan block '{blockId}' was not found.");

        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        definition.Blocks[index] = NormalizeBlock(block, blockId, definition.Blocks[index].Order);

        return SaveAndCreate(definition);
    }

    public TrainingPlan RemoveTrainingPlanBlock(string planId, string blockId)
    {
        if (string.IsNullOrWhiteSpace(blockId))
            throw new ArgumentException("Training plan block id must not be empty.", nameof(blockId));

        var definition = GetDefinition(planId);
        var removed = definition.Blocks.RemoveAll(block =>
            string.Equals(block.Id, blockId, StringComparison.OrdinalIgnoreCase));

        if (removed == 0)
            throw new KeyNotFoundException($"Training plan block '{blockId}' was not found.");

        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        return SaveAndCreate(definition);
    }

    public void SaveTrainingPlanBlocks(string planId, IEnumerable<TrainingPlanBlockDefinition> blocks)
    {
        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentException("Training plan id must not be empty.", nameof(planId));
        if (blocks is null)
            throw new ArgumentNullException(nameof(blocks));

        var definitions = _trainingPlanRepository.Load();
        var definition = definitions.FirstOrDefault(plan => string.Equals(plan.Id, planId, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Training plan '{planId}' was not found.");

        definition.SchemaVersion = Math.Max(definition.SchemaVersion, 2);
        definition.Blocks = blocks
            .Where(block => block.SessionIds.Count > 0)
            .Select(block => new TrainingPlanBlockDefinition
            {
                Id = block.Id,
                Name = block.Name,
                Focus = block.Focus,
                Structure = block.Structure,
                Description = block.Description,
                Order = block.Order,
                SessionIds = block.SessionIds
                    .Where(sessionId => !string.IsNullOrWhiteSpace(sessionId))
                    .Select(sessionId => sessionId.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .ToList();

        _trainingPlanFactory.Create(definition);
        _trainingPlanRepository.Save(definition);
    }

    private TrainingPlanDefinition GetDefinition(string planId)
    {
        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentException("Training plan id must not be empty.", nameof(planId));

        return _trainingPlanRepository.Load()
            .FirstOrDefault(plan => string.Equals(plan.Id, planId, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Training plan '{planId}' was not found.");
    }

    private TrainingPlan SaveAndCreate(TrainingPlanDefinition definition)
    {
        var plan = _trainingPlanFactory.Create(definition);
        _trainingPlanRepository.Save(definition);
        return plan;
    }

    private static TrainingSessionDefinition NormalizeSession(
        TrainingSessionDefinition session,
        string sessionId)
    {
        var date = session.Date != default
            ? session.Date.Date
            : session.Appointment?.StartsAt?.Date ?? default;

        return new TrainingSessionDefinition
        {
            Id = sessionId.Trim(),
            Name = ResolveSessionName(session).Trim(),
            Date = date,
            Runs = session.Runs,
            Workouts = session.Workouts,
            Warmup = session.Warmup,
            Drills = session.Drills,
            TrainingEffect = session.TrainingEffect,
            Appointment = session.Appointment
        };
    }

    private static TrainingPlanBlockDefinition NormalizeBlock(
        TrainingPlanBlockDefinition block,
        string blockId,
        int order)
    {
        return new TrainingPlanBlockDefinition
        {
            Id = blockId.Trim(),
            Name = block.Name?.Trim() ?? string.Empty,
            Focus = block.Focus?.Trim() ?? string.Empty,
            Structure = block.Structure?.Trim() ?? string.Empty,
            Description = block.Description?.Trim() ?? string.Empty,
            Order = order,
            SessionIds = block.SessionIds
                .Where(sessionId => !string.IsNullOrWhiteSpace(sessionId))
                .Select(sessionId => sessionId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static string ResolveSessionName(TrainingSessionDefinition session)
    {
        if (!string.IsNullOrWhiteSpace(session.Name))
            return session.Name;

        return session.Runs.FirstOrDefault() switch
        {
            PaceLetics.TrainingModule.CodeBase.Running.Definitions.PlannedSessionDefinition planned
                when !string.IsNullOrWhiteSpace(planned.Name) => planned.Name,
            PaceLetics.TrainingModule.CodeBase.Running.Definitions.IntervalSessionDefinition interval
                when !string.IsNullOrWhiteSpace(interval.Name) => interval.Name,
            _ => "Trainingseinheit"
        };
    }

    private static int NextBlockOrder(TrainingPlanDefinition definition)
    {
        return definition.Blocks.Count == 0 ? 1 : definition.Blocks.Max(block => block.Order) + 1;
    }

    private static string CreateUniqueId(IEnumerable<string> existingIds, string value)
    {
        var baseId = ToSlug(value);
        var candidate = baseId;
        var suffix = 2;
        var existing = existingIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        while (existing.Contains(candidate))
        {
            candidate = $"{baseId}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string ToSlug(string value)
    {
        var characters = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        var slug = string.Join("-", new string(characters).Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }

    private static string ToReadableName(string id)
    {
        var name = id.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
