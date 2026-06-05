using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

public sealed record UploadRecordingRequest(
    RunningAnalysisEvent AnalysisEvent,
    RunningAnalysisParticipant Participant,
    RunningAnalysisRecording Recording,
    Stream Content);
