using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.RunningAnalysis;

public sealed class UserDriveFolderReferenceDocument : IQueryItem
{
    public const string PartitionKeyValue = "user-drive-folders";

    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = PartitionKeyValue;
    public string AthleteUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FolderId { get; set; } = string.Empty;
    public string FolderUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = RunningAnalysisDocumentTypes.UserFolderReference;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
