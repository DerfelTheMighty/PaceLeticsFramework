using PaceLetics.TrainingPlanModule.CodeBase.Definitions;

namespace PaceLetics.TrainingPlanModule.CodeBase.Interfaces;

public interface ITrainingPlanDefinitionValidator
{
    void Validate(TrainingPlanDefinition definition);
}
