using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IUserDriveFolderRegistry
{
    Task<DriveFolderReference?> FindReusableFolderAsync(
        ReusableDriveFolderRequest request,
        CancellationToken cancellationToken = default);

    Task SaveFolderReferenceAsync(
        SaveDriveFolderReferenceRequest request,
        CancellationToken cancellationToken = default);
}
