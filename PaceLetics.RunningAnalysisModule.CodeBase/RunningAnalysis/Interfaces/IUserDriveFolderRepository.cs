using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IUserDriveFolderRepository
{
    Task<DriveFolderReference?> FindUserFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);

    Task SaveUserFolderAsync(
        SaveUserDriveFolderRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteUserFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);
}
