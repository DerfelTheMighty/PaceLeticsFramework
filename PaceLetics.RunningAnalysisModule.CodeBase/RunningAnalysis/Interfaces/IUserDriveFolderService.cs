using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IUserDriveFolderService
{
    Task<DriveFolderReference?> GetFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);

    Task<DriveFolderReference> CreateFolderAsync(
        UserDriveFolderRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);
}
