using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;

public interface IRunningAnalysisRepository
{
    Task<RunningAnalysisEvent?> GetEventAsync(string analysisEventId, CancellationToken cancellationToken = default);

    Task<RunningAnalysisEvent?> GetEventByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default);

    Task UpsertEventAsync(RunningAnalysisEvent analysisEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsAsync(string analysisEventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsForAthleteAsync(string athleteUserId, CancellationToken cancellationToken = default);

    Task<RunningAnalysisParticipant?> GetParticipantAsync(string analysisEventId, string athleteUserId, CancellationToken cancellationToken = default);

    Task<RunningAnalysisParticipant?> GetParticipantByIdAsync(string participantId, CancellationToken cancellationToken = default);

    Task UpsertParticipantAsync(RunningAnalysisParticipant participant, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RunningAnalysisRecording>> GetRecordingsForParticipantAsync(string participantId, CancellationToken cancellationToken = default);

    Task UpsertRecordingAsync(RunningAnalysisRecording recording, CancellationToken cancellationToken = default);
}
