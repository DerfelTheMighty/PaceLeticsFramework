using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IRunningAnalysisStorageProvider
{
    Task<DriveFolderReference> EnsureEventFolderAsync(
        RunningAnalysisEvent analysisEvent,
        CancellationToken cancellationToken = default);

    Task<DriveFolderReference> EnsureParticipantFolderAsync(
        RunningAnalysisEvent analysisEvent,
        RunningAnalysisParticipant participant,
        DriveFolderReference eventFolder,
        CancellationToken cancellationToken = default);

    Task GrantParticipantWriteAccessAsync(
        DriveFolderReference participantFolder,
        string participantEmail,
        CancellationToken cancellationToken = default);

    Task<DriveFileReference> UploadRecordingAsync(
        UploadRecordingRequest request,
        CancellationToken cancellationToken = default);
}
