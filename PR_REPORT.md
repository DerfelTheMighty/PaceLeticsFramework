# PR Report: Persistent Message Feed State

## Summary
- Reworked the athlete message feed so read and deleted message state is persisted server-side per user.
- Added `IsRead` / `IsUnread` state to `AthleteMessage`, while keeping message generation provider-driven.
- Replaced the right-anchored `MudMenu` feed with a viewport-centered overlay to prevent the panel from being clipped on the right side.
- Added a delete action to each feed message, with localized tooltip/ARIA labels.
- Updated the feed badge and warning button state so only unread messages ask for user attention.

## Implementation Details
- Added a Cosmos-backed message feed state store:
  - `AthleteMessageFeedStateDocument`
  - `IAthleteMessageFeedStateStore`
  - `CosmosAthleteMessageFeedStateStore`
- Persisted user state stores stable generated message IDs with read/deleted flags and timestamps.
- The feed still rebuilds current messages from `IAthleteMessageProvider` implementations; persisted state is applied afterward.
- Deleted messages are filtered out when the feed is rendered.
- Opening the feed marks currently visible unread messages as read and persists that state.
- Deleting a message marks it read and deleted, removes it from the current feed immediately, and persists the change.

## UI Changes
- The feed panel is now fixed-positioned and horizontally centered in the viewport.
- The feed width remains responsive via `min(390px, calc(100vw - 24px))`.
- Feed tiles now separate the navigation link area from the delete icon, preventing delete clicks from triggering navigation.
- Read messages render with normal emphasis; unread messages retain stronger tile emphasis.

## Configuration
- Registered `IAthleteMessageFeedStateStore` in DI.
- Fixed `AthleteDataOptions` binding so `CourseContainerName` is read from configuration instead of always falling back to the default.

## Verification
- `dotnet test PaceLetics.Tests\PaceLetics.Tests.csproj --no-restore` succeeded.
- Added unit coverage for default unread state, feed state document IDs, read state, and delete state.
- Browser verification on `http://localhost:5117` confirmed:
  - The feed overlay is centered (`centerDelta = 0`).
  - The overlay is not clipped left or right.
  - Delete buttons render for feed messages.
  - Opening the feed marks messages as read.
  - Deleting a message removes it immediately and the deletion remains after reload.

## Notes
- Existing NuGet vulnerability warnings for `OpenMcdf` and `SharpCompress` still appear during test/build and are unrelated to this change.
- Existing component analyzer/nullability warnings remain unrelated to this change.
