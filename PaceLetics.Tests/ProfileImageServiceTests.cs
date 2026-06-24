using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using PaceLetics.Web.Services.ProfileImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PaceLetics.Tests;

public sealed class ProfileImageServiceTests
{
    [Fact]
    public async Task SaveAsync_CropsToSquareAndScalesDown()
    {
        var root = CreateTempDirectory();
        var webRoot = Path.Combine(root, "wwwroot");
        Directory.CreateDirectory(webRoot);

        try
        {
            var service = new ProfileImageService(new TestWebHostEnvironment(root, webRoot));
            var upload = await CreateUploadAsync(1000, 600);

            var result = await service.SaveAsync(upload, "user-1", previousImageUrl: null);

            var savedPath = Path.Combine(webRoot, result.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(savedPath));

            using var saved = await Image.LoadAsync(savedPath);
            Assert.Equal(ProfileImageService.AvatarMaxSize, saved.Width);
            Assert.Equal(ProfileImageService.AvatarMaxSize, saved.Height);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_DeletesPreviousLocalImageAfterSuccessfulUpload()
    {
        var root = CreateTempDirectory();
        var webRoot = Path.Combine(root, "wwwroot");
        var uploadRoot = Path.Combine(webRoot, "uploads", "profile-images");
        Directory.CreateDirectory(uploadRoot);
        var previousPath = Path.Combine(uploadRoot, "old.webp");
        await File.WriteAllTextAsync(previousPath, "old");

        try
        {
            var service = new ProfileImageService(new TestWebHostEnvironment(root, webRoot));
            var upload = await CreateUploadAsync(128, 128);

            await service.SaveAsync(upload, "user-1", "/uploads/profile-images/old.webp");

            Assert.False(File.Exists(previousPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<IFormFile> CreateUploadAsync(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, Color.CornflowerBlue);
        var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "profileImage", "profile.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"paceletics-profile-images-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath, string webRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = webRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
            WebRootFileProvider = new PhysicalFileProvider(webRootPath);
        }

        public string ApplicationName { get; set; } = "PaceLetics.Tests";

        public IFileProvider ContentRootFileProvider { get; set; }

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; } = "Test";

        public IFileProvider WebRootFileProvider { get; set; }

        public string WebRootPath { get; set; }
    }
}
