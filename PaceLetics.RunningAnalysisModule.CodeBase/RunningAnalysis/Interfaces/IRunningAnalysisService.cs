using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IRunningAnalysisService
{
    Task<RunningAnalysisEvent> PrepareEventAsync(
        RunningAnalysisEventRequest request,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisParticipant> RegisterParticipantAsync(
        RunningAnalysisRegistration registration,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisRosterItem>> GetRosterAsync(
        string analysisEventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisLink>> GetAnalysesForAthleteAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default);

    Task HideAnalysisForAthleteAsync(
        string athleteUserId,
        string analysisEventId,
        CancellationToken cancellationToken = default);

    Task<RunningAnalysisEvent> StartAnalysisAsync(
        string analysisEventId,
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
