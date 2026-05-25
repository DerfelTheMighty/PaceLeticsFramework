using PaceLetics.RunningModule.CodeBase.Models;

namespace PaceLetics.RunningModule.CodeBase.Repositories;

public interface IRunningSessionRepository
{
    IReadOnlyList<RunningSessionDto> Load();
}
