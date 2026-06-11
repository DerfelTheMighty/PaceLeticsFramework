using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Services;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Storage;

namespace PaceLetics.Tests;

public sealed class RunningAnalysisServiceTests
{
    [Fact]
    public async Task PrepareEvent_CreatesPreparedEvent()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var service = CreateService(repository, new FakeRunningAnalysisStorageProvider(), new FakeUserDriveFolderRegistry());

        var analysisEvent = await service.PrepareEventAsync(CreateEventRequest());

        Assert.Equal("course-event-1", analysisEvent.ExternalEventId);
        Assert.Equal("course-1", analysisEvent.CourseId);
        Assert.Equal(RunningAnalysisEventStatus.Prepared, analysisEvent.Status);
        Assert.Single(repository.Events.Values);
    }

    [Fact]
    public async Task PrepareEvent_UpdatesExistingEventWithoutResettingProgress()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var service = CreateService(repository, new FakeRunningAnalysisStorageProvider(), new FakeUserDriveFolderRegistry());
        var analysisEvent = await service.PrepareEventAsync(CreateEventRequest());
        await service.StartAnalysisAsync(analysisEvent.Id);

        var updated = await service.PrepareEventAsync(CreateEventRequest(title: "Updated analysis"));

        Assert.Equal(analysisEvent.Id, updated.Id);
        Assert.Equal("Updated analysis", updated.Title);
        Assert.Equal(RunningAnalysisEventStatus.InProgress, updated.Status);
        Assert.Single(repository.Events.Values);
    }

    [Fact]
    public async Task RegisterParticipant_CreatesFolderAndGrantsWriteAccess()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var registry = new FakeUserDriveFolderRegistry();
        var userDrive = new FakeUserDriveFolderService();
        var service = CreateService(repository, storage, registry, userDrive);

        var participant = await service.RegisterParticipantAsync(CreateRegistration());

        Assert.Equal(RunningAnalysisFolderStatus.Ready, participant.FolderStatus);
        Assert.Equal(RunningAnalysisPermissionStatus.Granted, participant.PermissionStatus);
        Assert.Equal("runner@example.com", Assert.Single(storage.GrantedEmails));
        Assert.Equal("user-folder-runner-1", participant.DriveFolderId);
        Assert.Equal("https://drive.test/user-folder-runner-1", participant.DriveFolderUrl);
        Assert.Single(userDrive.CreatedFolders);
        Assert.Single(registry.SavedReferences);
    }

    [Fact]
    public async Task RegisterParticipant_ReusesExistingPersonalFolder()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var registry = new FakeUserDriveFolderRegistry();
        var userDrive = new FakeUserDriveFolderService
        {
            ExistingFolder = new DriveFolderReference("existing-folder", "https://drive.test/existing-folder")
        };
        var service = CreateService(repository, storage, registry, userDrive);

        var participant = await service.RegisterParticipantAsync(CreateRegistration());

        Assert.Equal("existing-folder", participant.DriveFolderId);
        Assert.Equal(0, storage.CreatedParticipantFolderCount);
        Assert.Empty(userDrive.CreatedFolders);
        Assert.Single(registry.SavedReferences);
        Assert.Equal(RunningAnalysisPermissionStatus.Granted, participant.PermissionStatus);
    }

    [Fact]
    public async Task RegisterParticipant_KeepsRegistrationWhenProvisioningFails()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var userDrive = new FakeUserDriveFolderService { FailCreate = true };
        var service = CreateService(repository, storage, new FakeUserDriveFolderRegistry(), userDrive);

        var participant = await service.RegisterParticipantAsync(CreateRegistration());

        Assert.Equal(RunningAnalysisFolderStatus.Failed, participant.FolderStatus);
        Assert.Equal(RunningAnalysisPermissionStatus.Failed, participant.PermissionStatus);
        Assert.Contains("folder failed", participant.ProvisioningError);

        var analysisEvent = Assert.Single(repository.Events.Values);
        var storedParticipant = await repository.GetParticipantAsync(analysisEvent.Id, "runner-1");
        Assert.NotNull(storedParticipant);
    }

    [Fact]
    public async Task UploadRecording_MarksOnlySuccessfulLatestUploadAsPrimary()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var service = CreateService(repository, storage, new FakeUserDriveFolderRegistry());
        var participant = await service.RegisterParticipantAsync(CreateRegistration());
        var analysisEvent = Assert.Single(repository.Events.Values);

        var first = await service.UploadRecordingAsync(
            analysisEvent.Id,
            participant.Id,
            "first.webm",
            "video/webm",
            new MemoryStream([1, 2, 3]),
            isOnline: true);
        var second = await service.UploadRecordingAsync(
            analysisEvent.Id,
            participant.Id,
            "second.webm",
            "video/webm",
            new MemoryStream([4, 5, 6]),
            isOnline: true);

        var recordings = await repository.GetRecordingsForParticipantAsync(participant.Id);
        Assert.False(recordings.Single(recording => recording.Id == first.Id).IsPrimary);
        Assert.True(recordings.Single(recording => recording.Id == second.Id).IsPrimary);
        Assert.Equal(RunningAnalysisUploadStatus.Uploaded, second.UploadStatus);
    }

    [Fact]
    public async Task UploadRecording_RefreshesPersonalFolderBeforeUpload()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var userDrive = new FakeUserDriveFolderService
        {
            ExistingFolder = new DriveFolderReference("personal-folder", "https://drive.test/personal-folder")
        };
        var service = CreateService(repository, storage, new FakeUserDriveFolderRegistry(), userDrive);
        var participant = await service.RegisterParticipantAsync(CreateRegistration());
        var analysisEvent = Assert.Single(repository.Events.Values);

        participant.DriveFolderId = "stale-folder";
        participant.DriveFolderUrl = "https://drive.test/stale-folder";
        await repository.UpsertParticipantAsync(participant);

        await service.UploadRecordingAsync(
            analysisEvent.Id,
            participant.Id,
            "video.webm",
            "video/webm",
            new MemoryStream([1, 2, 3]),
            isOnline: true);

        var storedParticipant = await repository.GetParticipantByIdAsync(participant.Id);
        Assert.Equal("personal-folder", storedParticipant?.DriveFolderId);
        Assert.Equal("personal-folder", Assert.Single(storage.UploadedFolderIds));
    }

    [Fact]
    public async Task UploadRecording_DoesNotPromoteFailedUploadToPrimary()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var service = CreateService(repository, storage, new FakeUserDriveFolderRegistry());
        var participant = await service.RegisterParticipantAsync(CreateRegistration());
        var analysisEvent = Assert.Single(repository.Events.Values);

        var first = await service.UploadRecordingAsync(
            analysisEvent.Id,
            participant.Id,
            "first.webm",
            "video/webm",
            new MemoryStream([1]),
            isOnline: true);
        storage.FailUpload = true;

        var failed = await service.UploadRecordingAsync(
            analysisEvent.Id,
            participant.Id,
            "failed.webm",
            "video/webm",
            new MemoryStream([2]),
            isOnline: true);

        var recordings = await repository.GetRecordingsForParticipantAsync(participant.Id);
        Assert.True(recordings.Single(recording => recording.Id == first.Id).IsPrimary);
        Assert.False(recordings.Single(recording => recording.Id == failed.Id).IsPrimary);
        Assert.Equal(RunningAnalysisUploadStatus.Failed, failed.UploadStatus);
    }

    [Fact]
    public async Task UploadRecording_RequiresOnlineConnection()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var storage = new FakeRunningAnalysisStorageProvider();
        var service = CreateService(repository, storage, new FakeUserDriveFolderRegistry());
        var participant = await service.RegisterParticipantAsync(CreateRegistration());
        var analysisEvent = Assert.Single(repository.Events.Values);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UploadRecordingAsync(
            analysisEvent.Id,
            participant.Id,
            "offline.webm",
            "video/webm",
            new MemoryStream([1]),
            isOnline: false));
    }

    [Fact]
    public async Task StartAndCompleteAnalysis_UpdatesEventStatus()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var service = CreateService(repository, new FakeRunningAnalysisStorageProvider(), new FakeUserDriveFolderRegistry());
        await service.RegisterParticipantAsync(CreateRegistration());
        var analysisEvent = Assert.Single(repository.Events.Values);

        await service.StartAnalysisAsync(analysisEvent.Id);
        Assert.Equal(RunningAnalysisEventStatus.InProgress, analysisEvent.Status);

        var completed = await service.CompleteAnalysisAsync(analysisEvent.Id);

        Assert.Equal(RunningAnalysisEventStatus.Completed, completed.Status);
    }

    [Fact]
    public async Task StartAnalysis_AllowsRestartingCompletedAnalysis()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var service = CreateService(repository, new FakeRunningAnalysisStorageProvider(), new FakeUserDriveFolderRegistry());
        await service.RegisterParticipantAsync(CreateRegistration());
        var analysisEvent = Assert.Single(repository.Events.Values);

        await service.CompleteAnalysisAsync(analysisEvent.Id);
        var restarted = await service.StartAnalysisAsync(analysisEvent.Id);

        Assert.Equal(RunningAnalysisEventStatus.InProgress, restarted.Status);
    }

    [Fact]
    public async Task GetAnalysesForAthlete_ReturnsFolderLinksForParticipant()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var service = CreateService(
            repository,
            new FakeRunningAnalysisStorageProvider(),
            new FakeUserDriveFolderRegistry());

        var participant = await service.RegisterParticipantAsync(CreateRegistration());

        var analyses = await service.GetAnalysesForAthleteAsync("runner-1");

        var analysis = Assert.Single(analyses);
        Assert.Equal(participant.DriveFolderUrl, analysis.DriveFolderUrl);
        Assert.Equal(RunningAnalysisFolderStatus.Ready, analysis.FolderStatus);
        Assert.Equal(RunningAnalysisPermissionStatus.Granted, analysis.PermissionStatus);
    }

    [Fact]
    public async Task HideAnalysisForAthlete_RemovesAnalysisFromAthleteOverview()
    {
        var repository = new InMemoryRunningAnalysisRepository();
        var service = CreateService(
            repository,
            new FakeRunningAnalysisStorageProvider(),
            new FakeUserDriveFolderRegistry());
        var participant = await service.RegisterParticipantAsync(CreateRegistration());

        await service.HideAnalysisForAthleteAsync("runner-1", participant.AnalysisEventId);

        var analyses = await service.GetAnalysesForAthleteAsync("runner-1");
        var storedParticipant = await repository.GetParticipantByIdAsync(participant.Id);
        Assert.Empty(analyses);
        Assert.True(storedParticipant?.IsHiddenFromAthlete);
    }

    private static RunningAnalysisService CreateService(
        InMemoryRunningAnalysisRepository repository,
        FakeRunningAnalysisStorageProvider storage,
        FakeUserDriveFolderRegistry registry,
        FakeUserDriveFolderService? userDrive = null)
    {
        return new RunningAnalysisService(
            repository,
            storage,
            registry,
            userDrive ?? new FakeUserDriveFolderService(),
            new FixedRunningAnalysisClock(new DateTime(2026, 6, 5, 10, 0, 0, DateTimeKind.Utc)));
    }

    private static RunningAnalysisRegistration CreateRegistration()
    {
        return new RunningAnalysisRegistration(
            ExternalEventId: "course-event-1",
            CourseId: "course-1",
            EventTitle: "Running analysis",
            StartsAt: new DateTime(2026, 6, 5, 18, 0, 0, DateTimeKind.Utc),
            EndsAt: new DateTime(2026, 6, 5, 20, 0, 0, DateTimeKind.Utc),
            AthleteUserId: "runner-1",
            DisplayName: "Runner One",
            Email: "runner@example.com",
            RegistrationId: "registration-1");
    }

    private static RunningAnalysisEventRequest CreateEventRequest(string title = "Running analysis")
    {
        return new RunningAnalysisEventRequest(
            ExternalEventId: "course-event-1",
            CourseId: "course-1",
            Title: title,
            StartsAt: new DateTime(2026, 6, 5, 18, 0, 0, DateTimeKind.Utc),
            EndsAt: new DateTime(2026, 6, 5, 20, 0, 0, DateTimeKind.Utc));
    }

    private sealed record FixedRunningAnalysisClock(DateTime UtcNow) : IRunningAnalysisClock;

    private sealed class FakeUserDriveFolderRegistry : IUserDriveFolderRegistry
    {
        public DriveFolderReference? ReusableFolder { get; set; }
        public List<SaveDriveFolderReferenceRequest> SavedReferences { get; } = new();

        public Task<DriveFolderReference?> FindReusableFolderAsync(
            ReusableDriveFolderRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReusableFolder);
        }

        public Task SaveFolderReferenceAsync(
            SaveDriveFolderReferenceRequest request,
            CancellationToken cancellationToken = default)
        {
            SavedReferences.Add(request);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserDriveFolderService : IUserDriveFolderService
    {
        public bool FailCreate { get; set; }
        public DriveFolderReference? ExistingFolder { get; set; }
        public List<UserDriveFolderRequest> CreatedFolders { get; } = new();

        public Task<DriveFolderReference?> GetFolderAsync(
            string athleteUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingFolder);
        }

        public Task<DriveFolderReference> CreateFolderAsync(
            UserDriveFolderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (FailCreate)
                throw new InvalidOperationException("folder failed");

            CreatedFolders.Add(request);
            ExistingFolder = new DriveFolderReference(
                $"user-folder-{request.AthleteUserId}",
                $"https://drive.test/user-folder-{request.AthleteUserId}");

            return Task.FromResult(ExistingFolder);
        }

        public Task<DriveFileReference> UploadAnalysisRecordingAsync(
            UserDriveAnalysisRecordingUploadRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DriveFileReference(
                $"file-{request.FileName}",
                $"https://drive.test/file-{request.FileName}"));
        }

        public Task DeleteFolderAsync(
            string athleteUserId,
            CancellationToken cancellationToken = default)
        {
            ExistingFolder = null;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRunningAnalysisStorageProvider : IRunningAnalysisStorageProvider
    {
        public bool FailFolderCreation { get; set; }
        public bool FailUpload { get; set; }
        public int CreatedParticipantFolderCount { get; private set; }
        public List<string> GrantedEmails { get; } = new();
        public List<string> UploadedFolderIds { get; } = new();

        public Task<DriveFolderReference> EnsureEventFolderAsync(
            RunningAnalysisEvent analysisEvent,
            CancellationToken cancellationToken = default)
        {
            if (FailFolderCreation)
                throw new InvalidOperationException("folder failed");

            return Task.FromResult(new DriveFolderReference("event-folder", "https://drive.test/event-folder"));
        }

        public Task<DriveFolderReference> EnsureParticipantFolderAsync(
            RunningAnalysisEvent analysisEvent,
            RunningAnalysisParticipant participant,
            DriveFolderReference eventFolder,
            CancellationToken cancellationToken = default)
        {
            CreatedParticipantFolderCount++;
            return Task.FromResult(new DriveFolderReference(
                $"participant-folder-{CreatedParticipantFolderCount}",
                $"https://drive.test/participant-folder-{CreatedParticipantFolderCount}"));
        }

        public Task GrantParticipantWriteAccessAsync(
            DriveFolderReference participantFolder,
            string participantEmail,
            CancellationToken cancellationToken = default)
        {
            GrantedEmails.Add(participantEmail);
            return Task.CompletedTask;
        }

        public Task<DriveFileReference> UploadRecordingAsync(
            UploadRecordingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (FailUpload)
                throw new InvalidOperationException("upload failed");

            UploadedFolderIds.Add(request.Participant.DriveFolderId!);
            return Task.FromResult(new DriveFileReference(
                $"file-{request.Recording.AttemptNumber}",
                $"https://drive.test/file-{request.Recording.AttemptNumber}"));
        }
    }

    private sealed class InMemoryRunningAnalysisRepository : IRunningAnalysisRepository
    {
        public Dictionary<string, RunningAnalysisEvent> Events { get; } = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RunningAnalysisParticipant> _participants = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RunningAnalysisRecording> _recordings = new(StringComparer.OrdinalIgnoreCase);

        public Task<RunningAnalysisEvent?> GetEventAsync(
            string analysisEventId,
            CancellationToken cancellationToken = default)
        {
            Events.TryGetValue(analysisEventId, out var analysisEvent);
            return Task.FromResult(analysisEvent);
        }

        public Task<RunningAnalysisEvent?> GetEventByExternalEventIdAsync(
            string externalEventId,
            CancellationToken cancellationToken = default)
        {
            var analysisEvent = Events.Values.FirstOrDefault(analysisEvent =>
                analysisEvent.ExternalEventId == externalEventId);
            return Task.FromResult(analysisEvent);
        }

        public Task UpsertEventAsync(
            RunningAnalysisEvent analysisEvent,
            CancellationToken cancellationToken = default)
        {
            Events[analysisEvent.Id] = analysisEvent;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsAsync(
            string analysisEventId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RunningAnalysisParticipant>>(
                _participants.Values.Where(participant => participant.AnalysisEventId == analysisEventId).ToList());
        }

        public Task<IReadOnlyList<RunningAnalysisParticipant>> GetParticipantsForAthleteAsync(
            string athleteUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RunningAnalysisParticipant>>(
                _participants.Values.Where(participant => participant.AthleteUserId == athleteUserId).ToList());
        }

        public Task<RunningAnalysisParticipant?> GetParticipantAsync(
            string analysisEventId,
            string athleteUserId,
            CancellationToken cancellationToken = default)
        {
            var participant = _participants.Values.FirstOrDefault(participant =>
                participant.AnalysisEventId == analysisEventId
                && participant.AthleteUserId == athleteUserId);
            return Task.FromResult(participant);
        }

        public Task<RunningAnalysisParticipant?> GetParticipantByIdAsync(
            string participantId,
            CancellationToken cancellationToken = default)
        {
            _participants.TryGetValue(participantId, out var participant);
            return Task.FromResult(participant);
        }

        public Task UpsertParticipantAsync(
            RunningAnalysisParticipant participant,
            CancellationToken cancellationToken = default)
        {
            _participants[participant.Id] = participant;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RunningAnalysisRecording>> GetRecordingsForParticipantAsync(
            string participantId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RunningAnalysisRecording>>(
                _recordings.Values.Where(recording => recording.ParticipantId == participantId).ToList());
        }

        public Task UpsertRecordingAsync(
            RunningAnalysisRecording recording,
            CancellationToken cancellationToken = default)
        {
            _recordings[recording.Id] = recording;
            return Task.CompletedTask;
        }
    }
}
