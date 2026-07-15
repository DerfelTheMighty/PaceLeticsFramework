using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.ProfileImages;

public sealed class ProfileImageDocument : IQueryItem
{
    public const string DocumentTypeValue = "profileImage";
    public const string PartitionKeyValue = "profile-images";

    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = PartitionKeyValue;
    public string DocumentType { get; set; } = DocumentTypeValue;
    public string UserId { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/webp";
    public byte[] Content { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
