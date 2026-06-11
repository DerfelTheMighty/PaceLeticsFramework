using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;

public sealed class RunningAnalysisParticipant
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentType { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string AnalysisEventId { get; set; } = string.Empty;
    public string AthleteUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RunningAnalysisFolderStatus FolderStatus { get; set; } = RunningAnalysisFolderStatus.Missing;
    public RunningAnalysisPermissionStatus PermissionStatus { get; set; } = RunningAnalysisPermissionStatus.Missing;
    public string? DriveFolderId { get; set; }
    public string? DriveFolderUrl { get; set; }
    public string? CreatedFromRegistrationId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string? ProvisioningError { get; set; }
    public bool IsHiddenFromAthlete { get; set; }
}
