using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Repositories;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Repositories;
using PaceLetics.Web.Services.Courses;
using PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces;

namespace PaceLetics.Web.Services;

public sealed class TrainingPlanService : ITrainingPlanService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IWorkoutCatalog _workoutCatalog;
    private readonly ICourseService _courseService;

    public TrainingPlanService(
        IWebHostEnvironment environment,
        IWorkoutCatalog workoutCatalog,
        ICourseService courseService)
    {
        _environment = environment;
        _workoutCatalog = workoutCatalog;
        _courseService = courseService;
    }

    public IReadOnlyList<TrainingPlan> LoadTrainingPlans()
    {
        var plansDir = Path.Combine(WebRootPath, "data", "plans");
        var plans = new JsonTrainingPlanRepository(plansDir, _workoutCatalog).Load().ToList();

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
        var legacyPath = Path.Combine(WebRootPath, "data", "intervalls.json");
        if (!File.Exists(legacyPath))
            return Array.Empty<RunningSession>();

        var definitions = new JsonRunningSessionRepository(legacyPath).Load();
        return RunningSessionFactory.Create(definitions);
    }

    private string WebRootPath =>
        !string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? _environment.WebRootPath
            : Path.Combine(AppContext.BaseDirectory, "wwwroot");

    private static string ToReadableName(string id)
    {
        var name = id.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
