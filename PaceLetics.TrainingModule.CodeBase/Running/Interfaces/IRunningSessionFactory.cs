using PaceLetics.TrainingModule.CodeBase.Running.Definitions;
using PaceLetics.TrainingModule.CodeBase.Running.Models;

namespace PaceLetics.TrainingModule.CodeBase.Running.Interfaces;

public interface IRunningSessionFactory
{
    RunningSession Create(RunningSessionDefinition definition);
    IReadOnlyList<RunningSession> Create(IEnumerable<RunningSessionDefinition> definitions);
}
