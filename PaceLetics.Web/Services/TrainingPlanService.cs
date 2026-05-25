using PaceLetics.RunningModule.CodeBase.Models;
using PaceLetics.RunningModule.CodeBase.Repositories;

namespace PaceLetics.Web.Services;

public sealed class TrainingPlanService : ITrainingPlanService
{
    private readonly IWebHostEnvironment _environment;

    public TrainingPlanService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<TrainingPlan> LoadTrainingPlans()
    {
        var plansDir = Path.Combine(WebRootPath, "data", "plans");
        var plans = new JsonTrainingPlanRepository(plansDir).Load().ToList();

        if (plans.Any())
            return plans;

        var sessions = LoadLegacySessions();
        if (sessions.Count == 0)
            return Array.Empty<TrainingPlan>();

        const string id = "intervalls";
        return new[] { new TrainingPlan(id, ToReadableName(id), sessions) };
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
