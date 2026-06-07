using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;

public sealed class UserDriveFolderService : IUserDriveFolderService
{
    private readonly IUserDriveFolderRepository _repository;
    private readonly IUserDriveFolderStorageProvider _storageProvider;

    public UserDriveFolderService(
        IUserDriveFolderRepository repository,
        IUserDriveFolderStorageProvider storageProvider)
    {
        _repository = repository;
        _storageProvider = storageProvider;
    }

    public Task<DriveFolderReference?> GetFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Task.FromResult<DriveFolderReference?>(null);

        return _repository.FindUserFolderAsync(athleteUserId.Trim(), cancellationToken);
    }

    public async Task<DriveFolderReference> CreateFolderAsync(
        UserDriveFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var existing = await _repository.FindUserFolderAsync(request.AthleteUserId.Trim(), cancellationToken);
        if (existing is not null)
            return existing;

        var userFolder = await _storageProvider.EnsureUserFolderAsync(request, cancellationToken);
        await _storageProvider.GrantUserReadAccessAsync(userFolder, request.Email, cancellationToken);
        await _repository.SaveUserFolderAsync(
            new SaveUserDriveFolderRequest(
                request.AthleteUserId.Trim(),
                request.Email.Trim(),
                userFolder.FolderId,
                userFolder.Url),
            cancellationToken);

        return userFolder;
    }

    public async Task DeleteFolderAsync(
        string athleteUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException("An athlete user id is required.");

        var existing = await _repository.FindUserFolderAsync(athleteUserId.Trim(), cancellationToken);
        if (existing is null)
            return;

        await _storageProvider.DeleteFolderAsync(existing, cancellationToken);
        await _repository.DeleteUserFolderAsync(athleteUserId.Trim(), cancellationToken);
    }

    private static void ValidateRequest(UserDriveFolderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AthleteUserId))
            throw new InvalidOperationException("An athlete user id is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("A user email is required to share the Drive folder.");
    }
}
