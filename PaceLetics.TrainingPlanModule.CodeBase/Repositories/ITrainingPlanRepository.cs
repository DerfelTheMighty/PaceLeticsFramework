using PaceLetics.TrainingPlanModule.CodeBase.Definitions;

namespace PaceLetics.TrainingPlanModule.CodeBase.Repositories;

public interface ITrainingPlanRepository
{
    IReadOnlyList<TrainingPlanDefinition> Load();
}
