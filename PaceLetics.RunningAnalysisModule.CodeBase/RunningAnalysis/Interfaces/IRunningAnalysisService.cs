using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IRunningAnalysisService
{
    Task<RunningAnalysisCaptureSession> PrepareCaptureSessionAsync(
        RunningAnalysisCaptureSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisEvent> PrepareEventAsync(
        RunningAnalysisEventRequest request,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisParticipant> RegisterParticipantAsync(
        RunningAnalysisRegistration registration,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisRosterItem>> GetRosterAsync(
        string captureSessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisCaptureLink>> GetCapturesForAthleteAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisLink>> GetAnalysesForAthleteAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);

    Task HideCaptureForAthleteAsync(
        string athleteUserId,
        string captureSessionId,
        CancellationToken cancellationToken = default);

    Task HideAnalysisForAthleteAsync(
        string athleteUserId,
        string analysisEventId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisResult?> GetAnalysisResultForParticipantAsync(
        string captureSessionId,
        string participantId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisCaptureSession> StartCaptureAsync(
        string captureSessionId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisEvent> StartAnalysisAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisCaptureSession> CompleteCaptureAsync(
        string captureSessionId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisEvent> CompleteAnalysisAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisRecording> UploadRecordingAsync(
        string analysisEventId,
        string participantId,
        string fileName,
        string contentType,
        Stream content,
        bool isOnline,
        RunningAnalysisPerspective perspective = RunningAnalysisPerspective.Side,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisRecording> RegisterUploadedRecordingAsync(
        string captureSessionId,
        string participantId,
        string fileName,
        string contentType,
        DriveFileReference driveFile,
        RunningAnalysisPerspective perspective = RunningAnalysisPerspective.Side,
        DateTime? recordedAt = null,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisResult> SaveAnalysisResultAsync(
        RunningAnalysisResultRequest request,
        CancellationToken cancellationToken = default);
}
