using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.Web.Services;

public interface ITrainingPlanService
{
    IReadOnlyList<TrainingPlan> LoadTrainingPlans();

    Task<IReadOnlyList<TrainingPlan>> LoadTrainingPlansForUserAsync(string? userId);

    IReadOnlyList<RunningSession> LoadLegacySessions();

    IReadOnlyList<TrainingPlanDefinition> LoadTrainingPlanDefinitions();

    TrainingPlan CreateTrainingPlan(string name);

    TrainingPlan UpdateTrainingPlan(string planId, string name);

    TrainingPlan AddTrainingSession(string planId, TrainingSessionDefinition session);

    TrainingPlan UpdateTrainingSession(string planId, string sessionId, TrainingSessionDefinition session);

    TrainingPlan RemoveTrainingSession(string planId, string sessionId);

    TrainingPlan AddTrainingPlanBlock(string planId, TrainingPlanBlockDefinition block);

    TrainingPlan UpdateTrainingPlanBlock(string planId, string blockId, TrainingPlanBlockDefinition block);

    TrainingPlan RemoveTrainingPlanBlock(string planId, string blockId);

    void SaveTrainingPlanBlocks(string planId, IEnumerable<TrainingPlanBlockDefinition> blocks);
}
