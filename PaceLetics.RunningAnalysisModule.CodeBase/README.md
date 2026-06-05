# Running Analysis Module

This module owns the running-analysis workflow independently from the course module.
The course/event system should integrate through a thin adapter that calls
`IRunningAnalysisService.RegisterParticipantAsync` after a participant registers
for an event of type `RunningAnalysis`.

## Implemented Core Rules

- A running-analysis event is created or updated from an external event id.
- Participant provisioning starts during registration.
- Existing participant folders can be reused through `IUserDriveFolderRegistry`.
- New participant folders are created in one central Drive area through
  `IRunningAnalysisStorageProvider`.
- Participant folders are shared with participant write access.
- Provisioning failures do not cancel the registration. The participant is stored
  with failed folder/permission status and can be retried by a later adapter.
- Recordings require an online connection.
- A recording only becomes primary after a successful upload.
- Failed uploads are stored, but they do not replace the current primary recording.

## Integration Ports

- `IRunningAnalysisRepository`
  Persists analysis events, participants, and recordings.
- `IRunningAnalysisStorageProvider`
  Creates Drive folders, grants permissions, and uploads recordings.
- `IUserDriveFolderRegistry`
  Finds reusable folders from this or other modules and records newly created
  participant folders.
- `IRunningAnalysisClock`
  Keeps time-dependent behavior testable.

## Next Adapters

- Course event adapter: call registration when an athlete registers for a running
  analysis event.
- Google Drive adapter: implement central root-folder creation, event folders,
  participant folders, write permission grants, and video uploads.
- Component module: trainer roster, browser camera capture, upload handoff, and
  participant "My analyses" folder-link view.
