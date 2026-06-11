using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed record RunningAnalysisRosterItem(
    string ParticipantId,
    string AthleteUserId,
    string AthleteEmail,
    string DisplayName,
    int SortOrder,
    RunningAnalysisFolderStatus FolderStatus,
    RunningAnalysisPermissionStatus PermissionStatus,
    int RecordingCount,
    string? PrimaryRecordingUrl);
