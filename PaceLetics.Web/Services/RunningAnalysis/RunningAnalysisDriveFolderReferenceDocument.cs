using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.RunningAnalysis;

public sealed class RunningAnalysisDriveFolderReferenceDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string ExternalEventId { get; set; } = string.Empty;
    public string AthleteUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FolderId { get; set; } = string.Empty;
    public string FolderUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = RunningAnalysisDocumentTypes.FolderReference;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
