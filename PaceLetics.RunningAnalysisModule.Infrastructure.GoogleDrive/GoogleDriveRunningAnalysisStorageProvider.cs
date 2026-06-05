using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;
using System.Text;

namespace PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive;

public sealed class GoogleDriveRunningAnalysisStorageProvider : IRunningAnalysisStorageProvider
{
    private const string FolderMimeType = "application/vnd.google-apps.folder";
    private readonly GoogleDriveRunningAnalysisOptions _options;

    public GoogleDriveRunningAnalysisStorageProvider(GoogleDriveRunningAnalysisOptions options)
    {
        _options = options;
    }

    public async Task<DriveFolderReference> EnsureEventFolderAsync(
        RunningAnalysisEvent analysisEvent,
        CancellationToken cancellationToken = default)
    {
        var drive = CreateDriveService();
        var rootFolder = await EnsureRootFolderAsync(drive, cancellationToken);
        var folderName = BuildEventFolderName(analysisEvent);

        return await EnsureFolderAsync(drive, folderName, rootFolder.FolderId, cancellationToken);
    }

    public Task<DriveFolderReference> EnsureParticipantFolderAsync(
        RunningAnalysisEvent analysisEvent,
        RunningAnalysisParticipant participant,
        DriveFolderReference eventFolder,
        CancellationToken cancellationToken = default)
    {
        var drive = CreateDriveService();
        var folderName = BuildParticipantFolderName(participant);

        return EnsureFolderAsync(drive, folderName, eventFolder.FolderId, cancellationToken);
    }

    public async Task GrantParticipantWriteAccessAsync(
        DriveFolderReference participantFolder,
        string participantEmail,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(participantEmail))
            throw new InvalidOperationException("A participant email is required to grant Drive access.");

        var drive = CreateDriveService();
        var permission = new Permission
        {
            Type = "user",
            Role = "writer",
            EmailAddress = participantEmail.Trim()
        };

        var request = drive.Permissions.Create(permission, participantFolder.FolderId);
        request.SendNotificationEmail = false;
        request.SupportsAllDrives = true;
        await request.ExecuteAsync(cancellationToken);
    }

    public async Task<DriveFileReference> UploadRecordingAsync(
        UploadRecordingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Participant.DriveFolderId))
            throw new InvalidOperationException("The participant Drive folder is required for uploads.");

        var drive = CreateDriveService();
        var metadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = request.Recording.FileName,
            Parents = new List<string> { request.Participant.DriveFolderId }
        };

        var upload = drive.Files.Create(
            metadata,
            request.Content,
            string.IsNullOrWhiteSpace(request.Recording.ContentType)
                ? "video/webm"
                : request.Recording.ContentType);

        upload.Fields = "id,webViewLink";
        upload.SupportsAllDrives = true;

        var result = await upload.UploadAsync(cancellationToken);
        if (result.Status != UploadStatus.Completed || upload.ResponseBody is null)
            throw result.Exception ?? new InvalidOperationException("Google Drive upload failed.");

        return new DriveFileReference(upload.ResponseBody.Id, upload.ResponseBody.WebViewLink);
    }

    private DriveService CreateDriveService()
    {
        GoogleCredential credential;

        if (!string.IsNullOrWhiteSpace(_options.ServiceAccountJson))
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_options.ServiceAccountJson));
            credential = ServiceAccountCredential.FromServiceAccountData(stream).ToGoogleCredential();
        }
        else if (!string.IsNullOrWhiteSpace(_options.ServiceAccountJsonPath))
        {
            using var stream = System.IO.File.OpenRead(_options.ServiceAccountJsonPath);
            credential = ServiceAccountCredential.FromServiceAccountData(stream).ToGoogleCredential();
        }
        else
        {
            throw new InvalidOperationException("Google Drive service account credentials are not configured.");
        }

        if (credential.IsCreateScopedRequired)
            credential = credential.CreateScoped(DriveService.Scope.Drive);

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = string.IsNullOrWhiteSpace(_options.ApplicationName)
                ? "PaceLetics"
                : _options.ApplicationName
        });
    }

    private async Task<DriveFolderReference> EnsureRootFolderAsync(
        DriveService drive,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.RootFolderId))
        {
            var get = drive.Files.Get(_options.RootFolderId);
            get.Fields = "id,webViewLink";
            get.SupportsAllDrives = true;
            var existing = await get.ExecuteAsync(cancellationToken);
            return new DriveFolderReference(existing.Id, existing.WebViewLink);
        }

        return await EnsureFolderAsync(
            drive,
            string.IsNullOrWhiteSpace(_options.RootFolderName)
                ? "PaceLetics Laufanalysen"
                : _options.RootFolderName,
            parentFolderId: null,
            cancellationToken);
    }

    private static async Task<DriveFolderReference> EnsureFolderAsync(
        DriveService drive,
        string folderName,
        string? parentFolderId,
        CancellationToken cancellationToken)
    {
        var existing = await FindFolderAsync(drive, folderName, parentFolderId, cancellationToken);
        if (existing is not null)
            return existing;

        var metadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = folderName,
            MimeType = FolderMimeType
        };

        if (!string.IsNullOrWhiteSpace(parentFolderId))
            metadata.Parents = new List<string> { parentFolderId };

        var create = drive.Files.Create(metadata);
        create.Fields = "id,webViewLink";
        create.SupportsAllDrives = true;
        var created = await create.ExecuteAsync(cancellationToken);

        return new DriveFolderReference(created.Id, created.WebViewLink);
    }

    private static async Task<DriveFolderReference?> FindFolderAsync(
        DriveService drive,
        string folderName,
        string? parentFolderId,
        CancellationToken cancellationToken)
    {
        var escapedName = EscapeQueryValue(folderName);
        var parentClause = string.IsNullOrWhiteSpace(parentFolderId)
            ? string.Empty
            : $" and '{EscapeQueryValue(parentFolderId)}' in parents";

        var list = drive.Files.List();
        list.Q = $"mimeType = '{FolderMimeType}' and trashed = false and name = '{escapedName}'{parentClause}";
        list.Fields = "files(id,name,webViewLink)";
        list.Spaces = "drive";
        list.SupportsAllDrives = true;
        list.IncludeItemsFromAllDrives = true;

        var result = await list.ExecuteAsync(cancellationToken);
        var folder = result.Files?.FirstOrDefault();

        return folder is null
            ? null
            : new DriveFolderReference(folder.Id, folder.WebViewLink);
    }

    private static string BuildEventFolderName(RunningAnalysisEvent analysisEvent)
    {
        return $"{analysisEvent.StartsAt:yyyy-MM-dd} {SanitizeName(analysisEvent.Title)}";
    }

    private static string BuildParticipantFolderName(RunningAnalysisParticipant participant)
    {
        var suffix = participant.AthleteUserId.Length <= 6
            ? participant.AthleteUserId
            : participant.AthleteUserId[^6..];

        return $"{SanitizeName(participant.DisplayName)} - {suffix}";
    }

    private static string SanitizeName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string(value
            .Trim()
            .Select(character => invalid.Contains(character) ? '-' : character)
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized)
            ? "Laufanalyse"
            : sanitized;
    }

    private static string EscapeQueryValue(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal);
    }
}
