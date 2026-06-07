using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IUserDriveFolderStorageProvider
{
    Task<DriveFolderReference> EnsureUserFolderAsync(
        UserDriveFolderRequest request,
        CancellationToken cancellationToken = default);

    Task GrantUserReadAccessAsync(
        DriveFolderReference userFolder,
        string userEmail,
        CancellationToken cancellationToken = default);

    Task DeleteFolderAsync(
        DriveFolderReference userFolder,
        CancellationToken cancellationToken = default);
}
