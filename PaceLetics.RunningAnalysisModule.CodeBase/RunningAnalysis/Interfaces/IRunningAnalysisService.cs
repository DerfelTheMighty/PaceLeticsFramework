using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

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
        CancellationToken cancellationToken = default);
}
