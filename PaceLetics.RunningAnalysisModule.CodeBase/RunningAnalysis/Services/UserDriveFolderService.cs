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
        {
            await _storageProvider.GrantUserReadAccessAsync(existing, request.Email, cancellationToken);
            return existing;
        }

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

    public async Task<DriveFileReference> UploadAnalysisRecordingAsync(
        UserDriveAnalysisRecordingUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUploadRequest(request);

        var athleteUserId = request.AthleteUserId.Trim();
        var userFolder = await _repository.FindUserFolderAsync(athleteUserId, cancellationToken);
        if (userFolder is null)
        {
            if (string.IsNullOrWhiteSpace(request.AthleteEmail))
                throw new InvalidOperationException("The athlete Drive folder was not found and the metadata does not contain an email address to create it.");

            userFolder = await CreateFolderAsync(
                new UserDriveFolderRequest(athleteUserId, request.AthleteEmail.Trim()),
                cancellationToken);
        }

        var analysisFolder = await _storageProvider.EnsureChildFolderAsync(
            userFolder,
            BuildAnalysisFolderName(request),
            cancellationToken);

        return await _storageProvider.UploadFileAsync(
            analysisFolder,
            request.FileName.Trim(),
            string.IsNullOrWhiteSpace(request.ContentType) ? "video/webm" : request.ContentType.Trim(),
            request.Content,
            cancellationToken);
    }

    public async Task<DriveFileReference> UploadAnalysisResultAsync(
        UserDriveAnalysisResultUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUploadRequest(request);

        var athleteUserId = request.AthleteUserId.Trim();
        var userFolder = await _repository.FindUserFolderAsync(athleteUserId, cancellationToken);
        if (userFolder is null)
        {
            if (string.IsNullOrWhiteSpace(request.AthleteEmail))
                throw new InvalidOperationException("The athlete Drive folder was not found and the result metadata does not contain an email address to create it.");

            userFolder = await CreateFolderAsync(
                new UserDriveFolderRequest(athleteUserId, request.AthleteEmail.Trim()),
                cancellationToken);
        }

        var analysisFolder = await _storageProvider.EnsureChildFolderAsync(
            userFolder,
            BuildAnalysisFolderName(request),
            cancellationToken);

        return await _storageProvider.UploadFileAsync(
            analysisFolder,
            request.FileName.Trim(),
            string.IsNullOrWhiteSpace(request.ContentType) ? "application/json" : request.ContentType.Trim(),
            request.Content,
            cancellationToken);
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
            throw new InvalidOperationException("A user email is required to create the Drive folder reference.");
    }

    private static void ValidateUploadRequest(UserDriveAnalysisRecordingUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AthleteUserId))
            throw new InvalidOperationException("The local recording metadata does not contain an athlete user id.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new InvalidOperationException("The local recording metadata does not contain a video file name.");

        if (request.Content is null)
            throw new ArgumentNullException(nameof(request.Content));
    }

    private static void ValidateUploadRequest(UserDriveAnalysisResultUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AthleteUserId))
            throw new InvalidOperationException("The analysis result metadata does not contain an athlete user id.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new InvalidOperationException("The analysis result metadata does not contain a file name.");

        if (request.Content is null)
            throw new ArgumentNullException(nameof(request.Content));
    }

    private static string BuildAnalysisFolderName(UserDriveAnalysisRecordingUploadRequest request)
    {
        return BuildAnalysisFolderName(request.AnalysisTitle, request.AnalysisStartsAt);
    }

    private static string BuildAnalysisFolderName(UserDriveAnalysisResultUploadRequest request)
    {
        return BuildAnalysisFolderName(request.AnalysisTitle, request.AnalysisStartsAt);
    }

    private static string BuildAnalysisFolderName(string analysisTitle, DateTime? analysisStartsAt)
    {
        var title = SanitizeName(analysisTitle);
        if (analysisStartsAt is { } captureStartsAt
            && title.StartsWith($"{captureStartsAt:yyyy-MM-dd} ", StringComparison.Ordinal))
        {
            return title;
        }

        if (analysisStartsAt is { } startsAt)
            return $"{startsAt:yyyy-MM-dd} {title}";

        return title;
    }

    private static string SanitizeName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string((value ?? string.Empty)
            .Trim()
            .Select(character => invalid.Contains(character) ? '-' : character)
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized)
            ? "Laufaufnahme"
            : sanitized;
    }
}
