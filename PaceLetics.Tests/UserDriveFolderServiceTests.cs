using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.Tests;

public sealed class UserDriveFolderServiceTests
{
    [Fact]
    public async Task CreateFolderAsync_ReturnsExistingFolderWithoutCreatingANewOne()
    {
        var repository = new FakeUserDriveFolderRepository
        {
            ExistingFolder = new DriveFolderReference("existing-folder", "https://drive.test/existing-folder")
        };
        var storage = new FakeUserDriveFolderStorageProvider();
        var service = new UserDriveFolderService(repository, storage);

        var folder = await service.CreateFolderAsync(new UserDriveFolderRequest(
            "athlete-1",
            "athlete@example.com"));

        Assert.Equal("existing-folder", folder.FolderId);
        Assert.Empty(storage.CreatedFolders);
        Assert.Equal("athlete@example.com", Assert.Single(storage.UserReadGrants).Email);
        Assert.Empty(repository.SavedFolders);
    }

    [Fact]
    public async Task CreateFolderAsync_CreatesSharesAndSavesFolder()
    {
        var repository = new FakeUserDriveFolderRepository();
        var storage = new FakeUserDriveFolderStorageProvider();
        var service = new UserDriveFolderService(repository, storage);

        var folder = await service.CreateFolderAsync(new UserDriveFolderRequest(
            "athlete-1",
            "athlete@example.com"));

        Assert.Equal("user-folder-athlete-1", folder.FolderId);
        Assert.Single(storage.CreatedFolders);
        Assert.Equal("athlete@example.com", Assert.Single(storage.UserReadGrants).Email);
        Assert.Single(repository.SavedFolders);
        Assert.Equal("athlete@example.com", repository.SavedFolders[0].Email);
    }

    [Fact]
    public async Task DeleteFolderAsync_DeletesDriveFolderAndReference()
    {
        var repository = new FakeUserDriveFolderRepository
        {
            ExistingFolder = new DriveFolderReference("existing-folder", "https://drive.test/existing-folder")
        };
        var storage = new FakeUserDriveFolderStorageProvider();
        var service = new UserDriveFolderService(repository, storage);

        await service.DeleteFolderAsync("athlete-1");

        Assert.Single(storage.DeletedFolders);
        Assert.Equal("existing-folder", storage.DeletedFolders[0].FolderId);
        Assert.Equal("athlete-1", repository.DeletedAthleteUserIds.Single());
    }

    [Fact]
    public async Task UploadAnalysisRecordingAsync_UploadsIntoAnalysisFolderInUserDrive()
    {
        var repository = new FakeUserDriveFolderRepository
        {
            ExistingFolder = new DriveFolderReference("user-folder", "https://drive.test/user-folder")
        };
        var storage = new FakeUserDriveFolderStorageProvider();
        var service = new UserDriveFolderService(repository, storage);

        var file = await service.UploadAnalysisRecordingAsync(
            new UserDriveAnalysisRecordingUploadRequest(
                "athlete-1",
                "athlete@example.com",
                "Laufanalyse Bahn",
                new DateTime(2026, 6, 11, 8, 0, 0, DateTimeKind.Utc),
                "video.webm",
                "video/webm",
                new MemoryStream([1, 2, 3])));

        Assert.Equal("https://drive.test/file-video.webm", file.Url);
        var childFolder = Assert.Single(storage.CreatedChildFolders);
        Assert.Equal("user-folder", childFolder.Parent.FolderId);
        Assert.Equal("2026-06-11 Laufanalyse Bahn", childFolder.FolderName);
        var uploaded = Assert.Single(storage.UploadedFiles);
        Assert.Equal("analysis-folder-1", uploaded.Folder.FolderId);
        Assert.Equal("video.webm", uploaded.FileName);
    }

    private sealed class FakeUserDriveFolderRepository : IUserDriveFolderRepository
    {
        public DriveFolderReference? ExistingFolder { get; set; }
        public List<SaveUserDriveFolderRequest> SavedFolders { get; } = new();
        public List<string> DeletedAthleteUserIds { get; } = new();

        public Task<DriveFolderReference?> FindUserFolderAsync(
            string athleteUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingFolder);
        }

        public Task SaveUserFolderAsync(
            SaveUserDriveFolderRequest request,
            CancellationToken cancellationToken = default)
        {
            SavedFolders.Add(request);
            ExistingFolder = new DriveFolderReference(request.FolderId, request.FolderUrl);
            return Task.CompletedTask;
        }

        public Task DeleteUserFolderAsync(
            string athleteUserId,
            CancellationToken cancellationToken = default)
        {
            DeletedAthleteUserIds.Add(athleteUserId);
            ExistingFolder = null;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserDriveFolderStorageProvider : IUserDriveFolderStorageProvider
    {
        public List<UserDriveFolderRequest> CreatedFolders { get; } = new();
        public List<DriveFolderReference> PublicReadLinkFolders { get; } = new();
        public List<(DriveFolderReference Folder, string Email)> UserReadGrants { get; } = new();
        public List<(DriveFolderReference Parent, string FolderName)> CreatedChildFolders { get; } = new();
        public List<(DriveFolderReference Folder, string FileName, string ContentType)> UploadedFiles { get; } = new();
        public List<DriveFolderReference> DeletedFolders { get; } = new();

        public Task<DriveFolderReference> EnsureUserFolderAsync(
            UserDriveFolderRequest request,
            CancellationToken cancellationToken = default)
        {
            CreatedFolders.Add(request);
            return Task.FromResult(new DriveFolderReference(
                $"user-folder-{request.AthleteUserId}",
                $"https://drive.test/user-folder-{request.AthleteUserId}"));
        }

        public Task EnsureUserFolderHasPublicReadLinkAsync(
            DriveFolderReference userFolder,
            CancellationToken cancellationToken = default)
        {
            PublicReadLinkFolders.Add(userFolder);
            return Task.CompletedTask;
        }

        public Task GrantUserReadAccessAsync(
            DriveFolderReference userFolder,
            string email,
            CancellationToken cancellationToken = default)
        {
            UserReadGrants.Add((userFolder, email));
            return Task.CompletedTask;
        }

        public Task<DriveFolderReference> EnsureChildFolderAsync(
            DriveFolderReference parentFolder,
            string folderName,
            CancellationToken cancellationToken = default)
        {
            CreatedChildFolders.Add((parentFolder, folderName));
            return Task.FromResult(new DriveFolderReference(
                $"analysis-folder-{CreatedChildFolders.Count}",
                $"https://drive.test/analysis-folder-{CreatedChildFolders.Count}"));
        }

        public Task<DriveFileReference> UploadFileAsync(
            DriveFolderReference folder,
            string fileName,
            string contentType,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            UploadedFiles.Add((folder, fileName, contentType));
            return Task.FromResult(new DriveFileReference(
                $"file-{fileName}",
                $"https://drive.test/file-{fileName}"));
        }

        public Task DeleteFolderAsync(
            DriveFolderReference userFolder,
            CancellationToken cancellationToken = default)
        {
            DeletedFolders.Add(userFolder);
            return Task.CompletedTask;
        }
    }
}
