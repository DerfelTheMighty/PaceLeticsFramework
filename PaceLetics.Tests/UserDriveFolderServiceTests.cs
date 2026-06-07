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
            "athlete@example.com",
            "Athlete One"));

        Assert.Equal("existing-folder", folder.FolderId);
        Assert.Empty(storage.CreatedFolders);
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
            "athlete@example.com",
            "Athlete One"));

        Assert.Equal("user-folder-athlete-1", folder.FolderId);
        Assert.Single(storage.CreatedFolders);
        Assert.Single(storage.SharedFolders);
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
        public List<(DriveFolderReference Folder, string Email)> SharedFolders { get; } = new();
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

        public Task GrantUserWriteAccessAsync(
            DriveFolderReference userFolder,
            string userEmail,
            CancellationToken cancellationToken = default)
        {
            SharedFolders.Add((userFolder, userEmail));
            return Task.CompletedTask;
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
