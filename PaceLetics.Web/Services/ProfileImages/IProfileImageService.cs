using Microsoft.AspNetCore.Http;

namespace PaceLetics.Web.Services.ProfileImages;

public interface IProfileImageService
{
    Task<ProfileImageSaveResult> SaveAsync(
        IFormFile file,
        string userId,
        string? previousImageUrl,
        CancellationToken cancellationToken = default);
}

public sealed record ProfileImageSaveResult(string Url);
