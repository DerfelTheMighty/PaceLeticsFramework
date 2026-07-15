using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace PaceLetics.Web.Services.ProfileImages;

public sealed class ProfileImageService : IProfileImageService
{
    public const int AvatarMaxSize = 512;
    public const long MaxUploadBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IProfileImageStore _store;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProfileImageService>? _logger;

    public ProfileImageService(
        IWebHostEnvironment environment,
        IProfileImageStore store,
        TimeProvider? timeProvider = null,
        ILogger<ProfileImageService>? logger = null)
    {
        _environment = environment;
        _store = store;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger;
    }

    public async Task<ProfileImageSaveResult> SaveAsync(
        IFormFile file,
        string userId,
        string? previousImageUrl,
        CancellationToken cancellationToken = default)
    {
        ValidateUpload(file);

        var imageId = $"profile-image:{SanitizeUserId(userId)}:{Guid.NewGuid():N}";
        byte[] content;

        try
        {
            await using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            var cropSize = Math.Min(image.Width, image.Height);
            if (cropSize <= 0)
            {
                throw new ProfileImageException(ProfileImageError.InvalidImage);
            }

            var outputSize = Math.Min(cropSize, AvatarMaxSize);
            var cropRectangle = new Rectangle(
                (image.Width - cropSize) / 2,
                (image.Height - cropSize) / 2,
                cropSize,
                cropSize);

            image.Mutate(context => context
                .Crop(cropRectangle)
                .Resize(outputSize, outputSize));

            await using var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = 82 }, cancellationToken);
            content = output.ToArray();
        }
        catch (ProfileImageException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ProfileImageException(ProfileImageError.InvalidImage);
        }

        await _store.SaveAsync(new ProfileImageDocument
        {
            Id = imageId,
            UserId = userId,
            Content = content,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        }, cancellationToken);

        try
        {
            await DeletePreviousImageAsync(previousImageUrl, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger?.LogWarning(exception, "The previous profile image could not be deleted after saving {ImageId}.", imageId);
        }

        return new ProfileImageSaveResult($"/profile-images/{Uri.EscapeDataString(imageId)}");
    }

    private static void ValidateUpload(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new ProfileImageException(ProfileImageError.Empty);
        }

        if (file.Length > MaxUploadBytes)
        {
            throw new ProfileImageException(ProfileImageError.TooLarge);
        }

        if (!SupportedContentTypes.Contains(file.ContentType))
        {
            throw new ProfileImageException(ProfileImageError.UnsupportedType);
        }
    }

    private string GetUploadRoot()
    {
        var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? _environment.WebRootPath
            : Path.Combine(_environment.ContentRootPath, "wwwroot");

        return Path.GetFullPath(Path.Combine(webRootPath, "uploads", "profile-images"));
    }

    private void DeletePreviousLocalImage(string? previousImageUrl)
    {
        if (string.IsNullOrWhiteSpace(previousImageUrl)
            || !previousImageUrl.StartsWith("/uploads/profile-images/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var uploadRoot = GetUploadRoot();
        var fileName = Path.GetFileName(previousImageUrl);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var path = Path.GetFullPath(Path.Combine(uploadRoot, fileName));
        if (!path.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        DeleteFileIfExists(path);
    }

    private async Task DeletePreviousImageAsync(string? previousImageUrl, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(previousImageUrl)
            && previousImageUrl.StartsWith("/profile-images/", StringComparison.OrdinalIgnoreCase))
        {
            var id = Uri.UnescapeDataString(previousImageUrl["/profile-images/".Length..]);
            if (!string.IsNullOrWhiteSpace(id))
                await _store.DeleteAsync(id, cancellationToken);
            return;
        }

        DeletePreviousLocalImage(previousImageUrl);
    }

    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string SanitizeUserId(string userId)
    {
        var safeCharacters = userId
            .Where(character => char.IsLetterOrDigit(character) || character is '-' or '_')
            .ToArray();

        return safeCharacters.Length == 0
            ? "user"
            : new string(safeCharacters);
    }
}
