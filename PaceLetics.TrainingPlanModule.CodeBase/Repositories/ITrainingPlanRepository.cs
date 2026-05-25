using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Repositories;

public interface ITrainingPlanRepository
{
    IReadOnlyList<TrainingPlan> Load();
}
