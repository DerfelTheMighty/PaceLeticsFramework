using PaceLetics.TrainingModule.CodeBase.Running.Definitions;

namespace PaceLetics.TrainingModule.CodeBase.Running.Repositories;

public interface IRunningSessionRepository
{
    IReadOnlyList<RunningSessionDefinition> Load();
}
