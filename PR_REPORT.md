# PR Report: User-Owned Google Drive Folder Management

## Summary
- Added a new authenticated **My Drive** area where users can manage their personal PaceLetics Google Drive folder.
- Added a navigation entry for **My Drive**.
- Added create, view, and delete flows for per-user Drive folders.
- Kept folder names anonymous by excluding email addresses and display names.
- Hardened Google Drive service account credential loading for local and hosted environments.

## User Experience
- Users can open `/Athletes/drive` from the navigation menu.
- If no folder exists, the page shows a **Create folder** action.
- If a folder exists, the page shows an **Open folder** link and a **Delete folder** action.
- Folder deletion remains controlled through the app button and service account flow.
- The folder link is not public. It works for the Google account that was explicitly granted access.

## Google Drive Behavior
- User folders are created below the configured PaceLetics Drive root.
- User folder names use the format:
  - `PaceLetics - User - {last 6 chars of AthleteUserId}`
- The user's email address is used only for permission assignment, not for folder naming.
- Users receive `reader` access to their folder.
- The service account remains responsible for folder lifecycle operations, including deletion.

## Credential Handling
- `ServiceAccountJsonPath` is preferred when it points to an existing credential file.
- `ServiceAccountJson` supports:
  - raw service account JSON
  - quoted or escaped JSON strings
  - base64-encoded service account JSON
  - a file path accidentally supplied in the JSON setting
- Invalid credential JSON now produces a clearer configuration error.

## Implementation Details
- Added user Drive folder contracts:
  - `IUserDriveFolderService`
  - `IUserDriveFolderRepository`
  - `IUserDriveFolderStorageProvider`
- Added request models:
  - `UserDriveFolderRequest`
  - `SaveUserDriveFolderRequest`
- Added `UserDriveFolderService` to coordinate repository and Google Drive operations.
- Extended the Google Drive storage provider with:
  - user folder creation
  - user read access grants
  - Drive folder deletion
  - robust credential normalization
- Extended the Cosmos running analysis repository with user folder references.
- Registered the new services in web dependency injection.
- Added localized My Drive page resources for English, German, and fallback resources.

## Tests
- Added `UserDriveFolderServiceTests` covering:
  - reusing an existing folder reference
  - creating, sharing, and saving a new folder reference
  - deleting the Drive folder and stored reference

## Verification
- `dotnet test PaceLeticsFramework.sln` passed.
- Test result: 111 passed, 0 failed, 0 skipped.
- Existing NuGet advisory warnings for `OpenMcdf` and `SharpCompress` still appear and are unrelated to this PR.

## PR
- Branch: `codex/user-drive-folder`
- Base: `main`
- Suggested title: `Add user-owned Google Drive folder management`
