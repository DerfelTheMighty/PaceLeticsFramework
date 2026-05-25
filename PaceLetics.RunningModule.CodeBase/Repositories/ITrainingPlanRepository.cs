using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.RunningModule.CodeBase.Repositories;

public interface ITrainingPlanRepository
{
    IReadOnlyList<TrainingPlan> Load();
}
