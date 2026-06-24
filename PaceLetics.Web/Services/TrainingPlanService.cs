using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Interfaces;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
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

    private static string ToReadableName(string id)
    {
        var name = id.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
