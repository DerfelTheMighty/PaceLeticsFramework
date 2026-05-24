using PaceLetics.RunningModule.CodeBase.Models;

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
        var plans = TrainingPlanProvider.LoadFromDirectory(plansDir).ToList();

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
        return File.Exists(legacyPath)
            ? RunningSessionFactory.Load(legacyPath)
            : Array.Empty<RunningSession>();
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
