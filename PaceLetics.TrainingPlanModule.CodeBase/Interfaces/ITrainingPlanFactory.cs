using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.TrainingPlanModule.CodeBase.Interfaces;

public interface ITrainingPlanFactory
{
    TrainingPlan Create(TrainingPlanDefinition definition);
    IReadOnlyList<TrainingPlan> Create(IEnumerable<TrainingPlanDefinition> definitions);
}
