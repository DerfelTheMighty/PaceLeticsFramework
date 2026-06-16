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
    string? DriveFolderUrl,
    string? ResultId = null,
    RunningAnalysisResultStatus? ResultStatus = null,
    DateTime? AnalyzedAt = null,
    string Summary = "",
    int? SideScore = null,
    int? SideMaxScore = null,
    int? RearScore = null,
    int? RearMaxScore = null,
    int? TotalScore = null,
    int? TotalMaxScore = null,
    string? SideRecordingUrl = null,
    string? RearRecordingUrl = null,
    string? ResultDriveFileUrl = null,
    IReadOnlyList<RunningAnalysisAssessmentItem>? SideAssessment = null,
    IReadOnlyList<RunningAnalysisAssessmentItem>? RearAssessment = null);
