using PaceLetics.RunningModule.CodeBase.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.Web.Services;

public interface ITrainingPlanService
{
    IReadOnlyList<TrainingPlan> LoadTrainingPlans();

    Task<IReadOnlyList<TrainingPlan>> LoadTrainingPlansForUserAsync(string? userId);

    IReadOnlyList<RunningSession> LoadLegacySessions();
}
