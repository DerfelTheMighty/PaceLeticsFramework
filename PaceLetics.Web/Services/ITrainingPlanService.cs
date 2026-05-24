using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.Web.Services;

public interface ITrainingPlanService
{
    IReadOnlyList<TrainingPlan> LoadTrainingPlans();

    IReadOnlyList<RunningSession> LoadLegacySessions();
}
