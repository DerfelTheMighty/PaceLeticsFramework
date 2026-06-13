using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Upload;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;
using System.Text;
using System.Text.Json;

namespace PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive;

public sealed class GoogleDriveRunningAnalysisStorageProvider :
    IRunningAnalysisStorageProvider,
    IUserDriveFolderStorageProvider
{
    private const string FolderMimeType = "application/vnd.google-apps.folder";
    private const string ServiceAccountQuotaFailureMessage = "Google Drive upload failed because the service account has no storage quota. Configure PaceLeticsUserData:GoogleDrive:DelegatedUserEmail for domain-wide delegation or use a shared-drive RootFolderId.";
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
        await GrantAccessAsync(drive, participantFolder, participantEmail, role: "writer", cancellationToken);
    }

    public async Task<DriveFolderReference> EnsureUserFolderAsync(
        UserDriveFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        var drive = CreateDriveService();
        var rootFolder = await EnsureRootFolderAsync(drive, cancellationToken);
        var folderName = BuildUserFolderName(request);

        return await EnsureFolderAsync(drive, folderName, rootFolder.FolderId, cancellationToken);
    }

    public async Task GrantUserReadAccessAsync(
        DriveFolderReference userFolder,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        var drive = CreateDriveService();
        await EnsureAnyoneWithLinkCanReadAsync(drive, userFolder.FolderId, cancellationToken);
        await GrantAccessAsync(drive, userFolder, userEmail, role: "reader", cancellationToken);
    }

    public async Task<DriveFolderReference> EnsureChildFolderAsync(
        DriveFolderReference parentFolder,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(parentFolder.FolderId))
            throw new InvalidOperationException("The parent Drive folder id is required.");

        if (string.IsNullOrWhiteSpace(folderName))
            throw new InvalidOperationException("The child Drive folder name is required.");

        var drive = CreateDriveService();
        return await EnsureFolderAsync(drive, SanitizeName(folderName), parentFolder.FolderId, cancellationToken);
    }

    public async Task<DriveFileReference> UploadFileAsync(
        DriveFolderReference folder,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folder.FolderId))
            throw new InvalidOperationException("The Drive folder id is required for uploads.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("The upload file name is required.");

        if (content is null)
            throw new ArgumentNullException(nameof(content));

        var drive = CreateDriveService();
        var metadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName.Trim(),
            Parents = new List<string> { folder.FolderId }
        };

        var upload = drive.Files.Create(
            metadata,
            content,
            string.IsNullOrWhiteSpace(contentType) ? "video/webm" : contentType.Trim());

        upload.Fields = "id,webViewLink,parents";
        upload.SupportsAllDrives = true;

        var result = await upload.UploadAsync(cancellationToken);
        if (result.Status != UploadStatus.Completed || upload.ResponseBody is null)
            throw CreateDriveUploadException(result);

        var uploadedFile = await EnsureFileIsInFolderAsync(
            drive,
            upload.ResponseBody,
            folder.FolderId,
            cancellationToken);

        return new DriveFileReference(uploadedFile.Id, uploadedFile.WebViewLink);
    }

    public async Task DeleteFolderAsync(
        DriveFolderReference userFolder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userFolder.FolderId))
            throw new InvalidOperationException("The Drive folder id is required.");

        var drive = CreateDriveService();
        var request = drive.Files.Delete(userFolder.FolderId);
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

        upload.Fields = "id,webViewLink,parents";
        upload.SupportsAllDrives = true;

        var result = await upload.UploadAsync(cancellationToken);
        if (result.Status != UploadStatus.Completed || upload.ResponseBody is null)
            throw CreateDriveUploadException(result);

        var uploadedFile = await EnsureFileIsInFolderAsync(
            drive,
            upload.ResponseBody,
            request.Participant.DriveFolderId,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Participant.Email))
        {
            await GrantAccessAsync(
                drive,
                new DriveFolderReference(uploadedFile.Id, uploadedFile.WebViewLink),
                request.Participant.Email,
                role: "writer",
                cancellationToken);
        }

        return new DriveFileReference(uploadedFile.Id, uploadedFile.WebViewLink);
    }

    private async Task GrantAccessAsync(
        DriveFolderReference folder,
        string email,
        string role,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("A user email is required to grant Drive access.");

        var drive = CreateDriveService();
        await GrantAccessAsync(drive, folder, email, role, cancellationToken);
    }

    private static async Task GrantAccessAsync(
        DriveService drive,
        DriveFolderReference folder,
        string email,
        string role,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        var existingPermissions = drive.Permissions.List(folder.FolderId);
        existingPermissions.Fields = "permissions(id,type,role,emailAddress,deleted)";
        existingPermissions.SupportsAllDrives = true;

        var permissions = await existingPermissions.ExecuteAsync(cancellationToken);
        var existingPermission = permissions.Permissions?
            .FirstOrDefault(permission =>
                string.Equals(permission.Type, "user", StringComparison.OrdinalIgnoreCase)
                && string.Equals(permission.EmailAddress, normalizedEmail, StringComparison.OrdinalIgnoreCase)
                && permission.Deleted != true);

        if (existingPermission is not null)
        {
            if (HasAtLeastRole(existingPermission.Role, role))
                return;

            var update = drive.Permissions.Update(
                new Permission
                {
                    Role = role
                },
                folder.FolderId,
                existingPermission.Id);

            update.SupportsAllDrives = true;
            await update.ExecuteAsync(cancellationToken);
            return;
        }

        var permission = new Permission
        {
            Type = "user",
            Role = role,
            EmailAddress = normalizedEmail
        };

        var request = drive.Permissions.Create(permission, folder.FolderId);
        request.SendNotificationEmail = false;
        request.SupportsAllDrives = true;
        await request.ExecuteAsync(cancellationToken);
    }

    private static bool HasAtLeastRole(string? existingRole, string requiredRole)
    {
        return RoleRank(existingRole) >= RoleRank(requiredRole);
    }

    private static async Task<Google.Apis.Drive.v3.Data.File> EnsureFileIsInFolderAsync(
        DriveService drive,
        Google.Apis.Drive.v3.Data.File uploadedFile,
        string folderId,
        CancellationToken cancellationToken)
    {
        var parents = uploadedFile.Parents?.ToList();
        if (parents is null)
        {
            var get = drive.Files.Get(uploadedFile.Id);
            get.Fields = "id,webViewLink,parents";
            get.SupportsAllDrives = true;
            var file = await get.ExecuteAsync(cancellationToken);
            parents = file.Parents?.ToList() ?? new List<string>();
            uploadedFile = file;
        }

        if (parents.Contains(folderId, StringComparer.Ordinal))
            return uploadedFile;

        var update = drive.Files.Update(new Google.Apis.Drive.v3.Data.File(), uploadedFile.Id);
        update.AddParents = folderId;

        var parentsToRemove = parents
            .Where(parent => !string.Equals(parent, folderId, StringComparison.Ordinal))
            .ToList();
        if (parentsToRemove.Count > 0)
            update.RemoveParents = string.Join(",", parentsToRemove);

        update.Fields = "id,webViewLink,parents";
        update.SupportsAllDrives = true;

        return await update.ExecuteAsync(cancellationToken);
    }

    private static int RoleRank(string? role)
    {
        return role?.ToLowerInvariant() switch
        {
            "owner" => 4,
            "organizer" => 3,
            "fileorganizer" => 3,
            "writer" => 2,
            "commenter" => 1,
            "reader" => 0,
            _ => -1
        };
    }

    private DriveService CreateDriveService()
    {
        var credential = CreateCredential();

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = string.IsNullOrWhiteSpace(_options.ApplicationName)
                ? "PaceLetics"
                : _options.ApplicationName
        });
    }

    private IConfigurableHttpClientInitializer CreateCredential()
    {
        if (HasOAuthCredentials())
            return CreateOAuthCredential();

        if (HasPartialOAuthCredentials())
            throw new InvalidOperationException("Google Drive OAuth credentials are incomplete. Configure OAuthClientId, OAuthClientSecret and OAuthRefreshToken.");

        return CreateServiceAccountCredential();
    }

    private UserCredential CreateOAuthCredential()
    {
        var clientSecrets = new ClientSecrets
        {
            ClientId = _options.OAuthClientId.Trim(),
            ClientSecret = _options.OAuthClientSecret.Trim()
        };
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = clientSecrets,
            Scopes = new[] { DriveService.Scope.DriveFile }
        });
        var token = new TokenResponse
        {
            RefreshToken = _options.OAuthRefreshToken.Trim()
        };
        var userId = string.IsNullOrWhiteSpace(_options.OAuthUserEmail)
            ? "paceletics-drive-oauth-user"
            : _options.OAuthUserEmail.Trim();

        return new UserCredential(flow, userId, token);
    }

    private GoogleCredential CreateServiceAccountCredential()
    {
        GoogleCredential credential;
        var serviceAccountJsonPath = _options.ServiceAccountJsonPath?.Trim();
        var serviceAccountJson = _options.ServiceAccountJson?.Trim();

        if (!string.IsNullOrWhiteSpace(serviceAccountJsonPath) && System.IO.File.Exists(serviceAccountJsonPath))
        {
            using var stream = System.IO.File.OpenRead(serviceAccountJsonPath);
            credential = ServiceAccountCredential.FromServiceAccountData(stream).ToGoogleCredential();
        }
        else if (!string.IsNullOrWhiteSpace(serviceAccountJson))
        {
            var normalizedJson = NormalizeServiceAccountJson(serviceAccountJson);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(normalizedJson));
            credential = ServiceAccountCredential.FromServiceAccountData(stream).ToGoogleCredential();
        }
        else if (!string.IsNullOrWhiteSpace(serviceAccountJsonPath))
        {
            throw new InvalidOperationException(
                $"Google Drive service account credential file was not found: {serviceAccountJsonPath}");
        }
        else
        {
            throw new InvalidOperationException("Google Drive service account credentials are not configured.");
        }

        if (credential.IsCreateScopedRequired)
            credential = credential.CreateScoped(DriveService.Scope.Drive);

        var delegatedUserEmail = _options.DelegatedUserEmail?.Trim();
        if (!string.IsNullOrWhiteSpace(delegatedUserEmail))
        {
            try
            {
                credential = credential.CreateWithUser(delegatedUserEmail);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    "Google Drive delegated user can only be used with service-account credentials.",
                    ex);
            }
        }

        return credential;
    }

    private bool HasOAuthCredentials()
    {
        return !string.IsNullOrWhiteSpace(_options.OAuthClientId)
            && !string.IsNullOrWhiteSpace(_options.OAuthClientSecret)
            && !string.IsNullOrWhiteSpace(_options.OAuthRefreshToken);
    }

    private bool HasPartialOAuthCredentials()
    {
        return !string.IsNullOrWhiteSpace(_options.OAuthClientId)
            || !string.IsNullOrWhiteSpace(_options.OAuthClientSecret)
            || !string.IsNullOrWhiteSpace(_options.OAuthRefreshToken)
            || !string.IsNullOrWhiteSpace(_options.OAuthUserEmail);
    }

    private static string NormalizeServiceAccountJson(string value)
    {
        var trimmed = value.Trim();

        if (System.IO.File.Exists(trimmed))
            return System.IO.File.ReadAllText(trimmed);

        var unescaped = TryUnescapeJsonString(trimmed);
        if (!string.Equals(unescaped, trimmed, StringComparison.Ordinal))
            trimmed = unescaped.Trim();

        if (!trimmed.StartsWith('{'))
            trimmed = TryDecodeBase64(trimmed) ?? trimmed;

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Google Drive service account credentials must be a JSON object.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Google Drive service account credentials are not valid JSON. Configure PaceLeticsUserData:GoogleDrive:ServiceAccountJsonPath with a readable credential file, or PaceLeticsUserData:GoogleDrive:ServiceAccountJson with raw or base64-encoded service account JSON.",
                ex);
        }

        return trimmed;
    }

    private static string TryUnescapeJsonString(string value)
    {
        if (!value.StartsWith('"') || !value.EndsWith('"'))
            return value;

        try
        {
            return JsonSerializer.Deserialize<string>(value) ?? value;
        }
        catch (JsonException)
        {
            return value;
        }
    }

    private static string? TryDecodeBase64(string value)
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            return decoded.TrimStart().StartsWith('{') ? decoded : null;
        }
        catch (FormatException)
        {
            return null;
        }
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
        {
            await EnsureAnyoneWithLinkCanReadAsync(drive, existing.FolderId, cancellationToken);
            return existing;
        }

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
        Google.Apis.Drive.v3.Data.File created;
        try
        {
            created = await create.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex) when (IsServiceAccountQuotaException(ex))
        {
            throw CreateServiceAccountQuotaException(ex);
        }

        await EnsureAnyoneWithLinkCanReadAsync(drive, created.Id, cancellationToken);

        return new DriveFolderReference(created.Id, created.WebViewLink);
    }

    private static async Task EnsureAnyoneWithLinkCanReadAsync(
        DriveService drive,
        string folderId,
        CancellationToken cancellationToken)
    {
        var existingPermissions = drive.Permissions.List(folderId);
        existingPermissions.Fields = "permissions(id,type,role,allowFileDiscovery)";
        existingPermissions.SupportsAllDrives = true;

        var permissions = await existingPermissions.ExecuteAsync(cancellationToken);
        var anyonePermission = permissions.Permissions?
            .FirstOrDefault(permission => string.Equals(permission.Type, "anyone", StringComparison.OrdinalIgnoreCase));

        if (anyonePermission is null)
        {
            var create = drive.Permissions.Create(
                new Permission
                {
                    Type = "anyone",
                    Role = "reader",
                    AllowFileDiscovery = false
                },
                folderId);

            create.SupportsAllDrives = true;
            await create.ExecuteAsync(cancellationToken);
            return;
        }

        if (string.Equals(anyonePermission.Role, "reader", StringComparison.OrdinalIgnoreCase)
            && anyonePermission.AllowFileDiscovery == false)
            return;

        var update = drive.Permissions.Update(
            new Permission
            {
                Role = "reader",
                AllowFileDiscovery = false
            },
            folderId,
            anyonePermission.Id);

        update.SupportsAllDrives = true;
        await update.ExecuteAsync(cancellationToken);
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

    private static string BuildUserFolderName(UserDriveFolderRequest request)
    {
        var suffix = request.AthleteUserId.Length <= 6
            ? request.AthleteUserId
            : request.AthleteUserId[^6..];

        return $"PaceLetics - User - {SanitizeName(suffix)}";
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

    private static Exception CreateDriveUploadException(IUploadProgress result)
    {
        if (IsServiceAccountQuotaException(result.Exception))
            return CreateServiceAccountQuotaException(result.Exception);

        return result.Exception ?? new InvalidOperationException("Google Drive upload failed.");
    }

    private static InvalidOperationException CreateServiceAccountQuotaException(Exception? innerException)
    {
        return new InvalidOperationException(ServiceAccountQuotaFailureMessage, innerException);
    }

    private static bool IsServiceAccountQuotaException(Exception? exception)
    {
        var message = exception?.Message ?? string.Empty;
        return message.Contains("Service Accounts do not have storage quota", StringComparison.OrdinalIgnoreCase)
            || message.Contains("service account has no storage quota", StringComparison.OrdinalIgnoreCase);
    }
}
