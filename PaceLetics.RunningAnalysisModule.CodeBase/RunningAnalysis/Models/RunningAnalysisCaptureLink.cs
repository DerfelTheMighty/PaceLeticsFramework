using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed record RunningAnalysisCaptureLink(
    string CaptureSessionId,
    string ExternalEventId,
    string Title,
    DateTime StartsAt,
    RunningAnalysisEventStatus CaptureStatus,
    RunningAnalysisFolderStatus FolderStatus,
    RunningAnalysisPermissionStatus PermissionStatus,
    string? DriveFolderUrl);
