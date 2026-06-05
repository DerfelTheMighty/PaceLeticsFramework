using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed record RunningAnalysisLink(
    string AnalysisEventId,
    string ExternalEventId,
    string Title,
    DateTime StartsAt,
    RunningAnalysisEventStatus EventStatus,
    RunningAnalysisFolderStatus FolderStatus,
    RunningAnalysisPermissionStatus PermissionStatus,
    string? DriveFolderUrl);
