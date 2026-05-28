using PaceLetics.TrainingModule.CodeBase.Running.Models;

namespace PaceLetics.TrainingModule.CodeBase.Running.Repositories;

public interface IRunningSessionRepository
{
    IReadOnlyList<RunningSessionDto> Load();
}
