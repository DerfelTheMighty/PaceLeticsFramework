using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Tests;

public sealed class CourseServiceTests
{
    [Fact]
    public async Task CreateCourse_AddsCreatorAsTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);

        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        Assert.Equal("trainer-1", course.CreatedByTrainerUserId);
        var trainer = Assert.Single(course.Trainers);
        Assert.Equal("trainer-1", trainer.TrainerUserId);
        Assert.Equal("Coach", trainer.DisplayName);
    }

    [Fact]
    public async Task CreateCourse_StoresFormattedCourseLevel()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);

        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Testkurs",
                Level = CourseLevel.Level3,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        Assert.Equal("Level 3", course.Level);
    }

    [Theory]
    [InlineData("1", "Level 1")]
    [InlineData("Level 2", "Level 2")]
    [InlineData("level 5", "Level 5")]
    [InlineData("Advanced", "Advanced")]
    public void CourseLevelFormatting_FormatsKnownLevelsAndPreservesUnknownValues(string level, string expected)
    {
        Assert.Equal(expected, CourseLevelFormatting.Format(level));
    }

    [Fact]
    public async Task DeleteCourse_RequiresCreator()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteCourseAsync(course.Id, "trainer-2"));

        Assert.NotNull(await repository.GetCourseAsync(course.Id));
    }

    [Fact]
    public async Task DeleteCourse_RemovesCourseForCreator()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        await service.DeleteCourseAsync(course.Id, "trainer-1");

        Assert.Null(await repository.GetCourseAsync(course.Id));
    }

    [Fact]
    public async Task AddTrainer_AddsTrainerWhenRequesterCanManageMembers()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        await service.AddTrainerAsync(course.Id, "trainer-2", "Second Coach", "trainer-1");
        var updatedCourse = await repository.GetCourseAsync(course.Id);

        Assert.NotNull(updatedCourse);
        Assert.Contains(updatedCourse.Trainers, trainer => trainer.TrainerUserId == "trainer-2");
    }

    [Fact]
    public async Task RemoveTrainer_AllowsAddedTrainerToLeave()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");
        await service.AddTrainerAsync(course.Id, "trainer-2", "Second Coach", "trainer-1");

        await service.RemoveTrainerAsync(course.Id, "trainer-2", "trainer-2");
        var updatedCourse = await repository.GetCourseAsync(course.Id);

        Assert.NotNull(updatedCourse);
        Assert.DoesNotContain(updatedCourse.Trainers, trainer => trainer.TrainerUserId == "trainer-2");
    }

    [Fact]
    public async Task RemoveTrainer_DoesNotAllowCreatorToLeave()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RemoveTrainerAsync(course.Id, "trainer-1", "trainer-1"));
    }

    [Fact]
    public async Task AddCourseDate_AddsDateForAssignedTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        var date = await service.AddCourseDateAsync(
            course.Id,
            "Techniktraining",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1",
            "Stadion");
        var updatedCourse = await repository.GetCourseAsync(course.Id);

        Assert.NotNull(updatedCourse);
        Assert.Contains(updatedCourse.Dates, existing => existing.Id == date.Id && existing.Location == "Stadion");
    }

    [Fact]
    public async Task AddCourseDate_RequiresTrainerWithEventPermissions()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddCourseDateAsync(
                course.Id,
                "Techniktraining",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1),
                "trainer-2"));
    }

    [Fact]
    public async Task CreateEvent_AddsEventForAssignedTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        var courseEvent = await service.CreateEventAsync(
            course.Id,
            "Startanalyse",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1");

        Assert.Contains(repository.Events, existing => existing.Id == courseEvent.Id);
    }

    [Fact]
    public async Task DeleteEvent_RemovesEventAndRegistrations()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");
        await service.JoinCourseAsync(course.Id, "athlete-1");
        var courseEvent = await service.CreateEventAsync(
            course.Id,
            "Startanalyse",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1");
        await service.RegisterForEventAsync(course.Id, courseEvent.Id, "athlete-1");

        await service.DeleteEventAsync(course.Id, courseEvent.Id, "trainer-1");

        Assert.DoesNotContain(repository.Events, existing => existing.Id == courseEvent.Id);
        Assert.Empty(await repository.GetEventRegistrationsAsync(course.Id, courseEvent.Id));
    }

    [Fact]
    public async Task GetEventRegistrationsForTrainer_ReturnsRegisteredParticipantsForAssignedTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");
        await service.JoinCourseAsync(course.Id, "athlete-1");
        await service.JoinCourseAsync(course.Id, "athlete-2");
        var courseEvent = await service.CreateEventAsync(
            course.Id,
            "Startanalyse",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1");
        await service.RegisterForEventAsync(course.Id, courseEvent.Id, "athlete-1");
        await service.RegisterForEventAsync(course.Id, courseEvent.Id, "athlete-2");
        await service.LeaveCourseAsync(course.Id, "athlete-2");

        var registrations = await service.GetEventRegistrationsForTrainerAsync(course.Id, courseEvent.Id, "trainer-1");

        var registration = Assert.Single(registrations);
        Assert.Equal("athlete-1", registration.AthleteUserId);
    }

    [Fact]
    public async Task GetEventRegistrationsForTrainer_RequiresAssignedTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");
        var courseEvent = await service.CreateEventAsync(
            course.Id,
            "Startanalyse",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetEventRegistrationsForTrainerAsync(course.Id, courseEvent.Id, "trainer-2"));
    }

    [Fact]
    public async Task PublishTrainingPlan_AddsPublicationForAssignedTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");

        await service.PublishTrainingPlanAsync(course.Id, "plan-1", "trainer-1");
        var updatedCourse = await repository.GetCourseAsync(course.Id);

        Assert.NotNull(updatedCourse);
        var publication = Assert.Single(updatedCourse.TrainingPlanPublications);
        Assert.Equal("plan-1", publication.TrainingPlanId);
        Assert.True(publication.ToContentPublication(course.Id).IsVisibleFor(FeedTarget.Course(course.Id), DateTime.UtcNow));
        Assert.Equal(FeedTargetTypes.Course, publication.Target?.NormalizeCopy().TargetType);
        Assert.Equal(course.Id, publication.Target?.NormalizeCopy().TargetId);
    }

    [Fact]
    public async Task RemoveTrainingPlanPublication_RemovesPublicationForAssignedTrainer()
    {
        var repository = new InMemoryCourseRepository();
        var service = new CourseService(repository);
        var course = await service.CreateCourseAsync(
            new CourseCreateRequest
            {
                Name = "Laufschule",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(14)
            },
            "trainer-1",
            "Coach");
        await service.PublishTrainingPlanAsync(course.Id, "plan-1", "trainer-1");

        await service.RemoveTrainingPlanPublicationAsync(course.Id, "plan-1", "trainer-1");
        var updatedCourse = await repository.GetCourseAsync(course.Id);

        Assert.NotNull(updatedCourse);
        Assert.Empty(updatedCourse.TrainingPlanPublications);
    }

    [Fact]
    public async Task JoinCourse_CreatesActiveEnrollment()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        var service = new CourseService(repository);

        var enrollment = await service.JoinCourseAsync("course-1", "athlete-1");

        Assert.Equal("course-1", enrollment.CourseId);
        Assert.Equal("athlete-1", enrollment.AthleteUserId);
        Assert.Equal(CourseEnrollmentStatus.Active, enrollment.Status);
    }

    [Fact]
    public async Task GetPublishedTrainingPlanIdsForAthlete_ReturnsPlansFromJoinedCourses()
    {
        var repository = new InMemoryCourseRepository(
            CreateCourse("course-1", "plan-1"),
            CreateCourse("course-2", "plan-2"));
        var service = new CourseService(repository);
        await service.JoinCourseAsync("course-2", "athlete-1");

        var planIds = await service.GetPublishedTrainingPlanIdsForAthleteAsync("athlete-1");

        Assert.Equal(new[] { "plan-2" }, planIds);
    }

    [Fact]
    public async Task GetPublishedTrainingPlanIdsForAthlete_IgnoresPublicationsForOtherFeedTargets()
    {
        var course = CreateCourse("course-1", "plan-1");
        course.TrainingPlanPublications[0].Target = FeedTarget.Course("course-2");
        var repository = new InMemoryCourseRepository(course);
        var service = new CourseService(repository);
        await service.JoinCourseAsync("course-1", "athlete-1");

        var planIds = await service.GetPublishedTrainingPlanIdsForAthleteAsync("athlete-1");

        Assert.Empty(planIds);
    }

    [Fact]
    public async Task LeaveCourse_CancelsEnrollmentAndHidesPublishedPlans()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        var service = new CourseService(repository);
        await service.JoinCourseAsync("course-1", "athlete-1");

        var enrollment = await service.LeaveCourseAsync("course-1", "athlete-1");
        var planIds = await service.GetPublishedTrainingPlanIdsForAthleteAsync("athlete-1");

        Assert.Equal(CourseEnrollmentStatus.Cancelled, enrollment.Status);
        Assert.NotNull(enrollment.CancelledAt);
        Assert.Empty(planIds);
    }

    [Fact]
    public async Task LeaveCourse_CancelsActiveEventRegistrations()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        repository.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = "course-1",
            Title = "Startanalyse",
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        var service = new CourseService(repository);
        await service.JoinCourseAsync("course-1", "athlete-1");
        await service.RegisterForEventAsync("course-1", "event-1", "athlete-1");

        await service.LeaveCourseAsync("course-1", "athlete-1");
        var registration = await repository.GetEventRegistrationAsync("course-1", "event-1", "athlete-1");

        Assert.NotNull(registration);
        Assert.Equal(CourseEventRegistrationStatus.Cancelled, registration.Status);
        Assert.NotNull(registration.CancelledAt);
    }

    [Fact]
    public async Task CancelEventRegistration_CancelsActiveRegistration()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        repository.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = "course-1",
            Title = "Startanalyse",
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        var service = new CourseService(repository);
        await service.JoinCourseAsync("course-1", "athlete-1");
        await service.RegisterForEventAsync("course-1", "event-1", "athlete-1");

        var registration = await service.CancelEventRegistrationAsync("course-1", "event-1", "athlete-1");

        Assert.Equal(CourseEventRegistrationStatus.Cancelled, registration.Status);
        Assert.NotNull(registration.CancelledAt);
    }

    [Fact]
    public async Task CancelEventRegistration_RequiresActiveRegistration()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        repository.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = "course-1",
            Title = "Startanalyse",
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        var service = new CourseService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelEventRegistrationAsync("course-1", "event-1", "athlete-1"));
    }

    [Fact]
    public async Task GetEventRegistrationForAthlete_ReturnsRegistration()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        repository.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = "course-1",
            Title = "Startanalyse",
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        var service = new CourseService(repository);
        await service.JoinCourseAsync("course-1", "athlete-1");
        await service.RegisterForEventAsync("course-1", "event-1", "athlete-1");

        var registration = await service.GetEventRegistrationForAthleteAsync("course-1", "event-1", "athlete-1");

        Assert.NotNull(registration);
        Assert.Equal(CourseEventRegistrationStatus.Registered, registration.Status);
    }

    [Fact]
    public async Task RegisterForEvent_RequiresCourseEnrollment()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        repository.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = "course-1",
            Title = "Startanalyse",
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1)
        });
        var service = new CourseService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterForEventAsync("course-1", "event-1", "athlete-1"));
    }

    [Fact]
    public async Task RegisterForEvent_NotifiesRunningAnalysisAdapterForRunningAnalysisEvent()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        var adapter = new FakeRunningAnalysisRegistrationAdapter();
        var service = new CourseService(repository, runningAnalysisRegistrationAdapter: adapter);
        await service.JoinCourseAsync("course-1", "athlete-1");
        var courseEvent = await service.CreateEventAsync(
            "course-1",
            "Laufanalyse",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1",
            eventType: CourseEventTypes.RunningAnalysis);

        await service.RegisterForEventAsync("course-1", courseEvent.Id, "athlete-1");

        var notification = Assert.Single(adapter.Notifications);
        Assert.Equal(courseEvent.Id, notification.CourseEvent.Id);
        Assert.Equal("athlete-1", notification.Registration.AthleteUserId);
    }

    [Fact]
    public async Task RegisterForEvent_DoesNotNotifyRunningAnalysisAdapterForGeneralEvent()
    {
        var repository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"));
        var adapter = new FakeRunningAnalysisRegistrationAdapter();
        var service = new CourseService(repository, runningAnalysisRegistrationAdapter: adapter);
        await service.JoinCourseAsync("course-1", "athlete-1");
        var courseEvent = await service.CreateEventAsync(
            "course-1",
            "Workshop",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "trainer-1");

        await service.RegisterForEventAsync("course-1", courseEvent.Id, "athlete-1");

        Assert.Empty(adapter.Notifications);
    }

    private static CourseDocument CreateCourse(string courseId, string planId)
    {
        return new CourseDocument
        {
            Id = courseId,
            CourseId = courseId,
            Name = courseId,
            Slug = courseId,
            IsPublished = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            CreatedByTrainerUserId = "trainer-1",
            Trainers = new List<CourseTrainerDocument>
            {
                new()
                {
                    TrainerUserId = "trainer-1",
                    DisplayName = "Coach"
                }
            },
            TrainingPlanPublications = new List<CourseTrainingPlanPublicationDocument>
            {
                new()
                {
                    TrainingPlanId = planId,
                    PublishedAt = DateTime.UtcNow.AddDays(-1)
                }
            }
        };
    }

    private sealed class InMemoryCourseRepository : ICourseRepository
    {
        private readonly List<CourseDocument> _courses;
        private readonly List<CourseEnrollmentDocument> _enrollments = new();
        private readonly List<CourseEventRegistrationDocument> _registrations = new();

        public InMemoryCourseRepository(params CourseDocument[] courses)
        {
            _courses = courses.ToList();
        }

        public List<CourseEventDocument> Events { get; } = new();

        public Task<IReadOnlyList<CourseDocument>> GetCoursesAsync()
        {
            return Task.FromResult<IReadOnlyList<CourseDocument>>(_courses);
        }

        public Task<CourseDocument?> GetCourseAsync(string courseId)
        {
            return Task.FromResult(_courses.FirstOrDefault(course => course.Id == courseId));
        }

        public Task UpsertCourseAsync(CourseDocument course)
        {
            _courses.RemoveAll(existing => existing.Id == course.Id);
            _courses.Add(course);
            return Task.CompletedTask;
        }

        public Task DeleteCourseAsync(string courseId)
        {
            _courses.RemoveAll(existing => existing.Id == courseId);
            _enrollments.RemoveAll(existing => existing.CourseId == courseId);
            Events.RemoveAll(existing => existing.CourseId == courseId);
            _registrations.RemoveAll(existing => existing.CourseId == courseId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CourseEnrollmentDocument>> GetEnrollmentsForAthleteAsync(string athleteUserId)
        {
            return Task.FromResult<IReadOnlyList<CourseEnrollmentDocument>>(
                _enrollments.Where(enrollment => enrollment.AthleteUserId == athleteUserId).ToList());
        }

        public Task<IReadOnlyList<CourseEnrollmentDocument>> GetEnrollmentsForCourseAsync(string courseId)
        {
            return Task.FromResult<IReadOnlyList<CourseEnrollmentDocument>>(
                _enrollments.Where(enrollment => enrollment.CourseId == courseId).ToList());
        }

        public Task<CourseEnrollmentDocument?> GetEnrollmentAsync(string courseId, string athleteUserId)
        {
            return Task.FromResult(_enrollments.FirstOrDefault(enrollment =>
                enrollment.CourseId == courseId && enrollment.AthleteUserId == athleteUserId));
        }

        public Task UpsertEnrollmentAsync(CourseEnrollmentDocument enrollment)
        {
            _enrollments.RemoveAll(existing =>
                existing.CourseId == enrollment.CourseId && existing.AthleteUserId == enrollment.AthleteUserId);
            _enrollments.Add(enrollment);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CourseEventDocument>> GetEventsAsync(string courseId)
        {
            return Task.FromResult<IReadOnlyList<CourseEventDocument>>(
                Events.Where(courseEvent => courseEvent.CourseId == courseId).ToList());
        }

        public Task<CourseEventDocument?> GetEventAsync(string courseId, string eventId)
        {
            return Task.FromResult(Events.FirstOrDefault(courseEvent =>
                courseEvent.CourseId == courseId && courseEvent.Id == eventId));
        }

        public Task UpsertEventAsync(CourseEventDocument courseEvent)
        {
            if (string.IsNullOrWhiteSpace(courseEvent.Id))
                courseEvent.Id = Guid.NewGuid().ToString("N");

            Events.RemoveAll(existing => existing.Id == courseEvent.Id);
            Events.Add(courseEvent);
            return Task.CompletedTask;
        }

        public Task DeleteEventAsync(string courseId, string eventId)
        {
            Events.RemoveAll(existing => existing.CourseId == courseId && existing.Id == eventId);
            _registrations.RemoveAll(existing => existing.CourseId == courseId && existing.EventId == eventId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsAsync(string courseId, string eventId)
        {
            return Task.FromResult<IReadOnlyList<CourseEventRegistrationDocument>>(
                _registrations.Where(registration =>
                    registration.CourseId == courseId && registration.EventId == eventId).ToList());
        }

        public Task<CourseEventRegistrationDocument?> GetEventRegistrationAsync(string courseId, string eventId, string athleteUserId)
        {
            return Task.FromResult(_registrations.FirstOrDefault(registration =>
                registration.CourseId == courseId
                && registration.EventId == eventId
                && registration.AthleteUserId == athleteUserId));
        }

        public Task UpsertEventRegistrationAsync(CourseEventRegistrationDocument registration)
        {
            _registrations.RemoveAll(existing =>
                existing.EventId == registration.EventId && existing.AthleteUserId == registration.AthleteUserId);
            _registrations.Add(registration);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRunningAnalysisRegistrationAdapter : ICourseRunningAnalysisRegistrationAdapter
    {
        public List<Notification> Notifications { get; } = new();

        public Task OnRegisteredAsync(
            CourseDocument course,
            CourseEventDocument courseEvent,
            CourseEventRegistrationDocument registration,
            CancellationToken cancellationToken = default)
        {
            Notifications.Add(new Notification(course, courseEvent, registration));
            return Task.CompletedTask;
        }
    }

    private sealed record Notification(
        CourseDocument Course,
        CourseEventDocument CourseEvent,
        CourseEventRegistrationDocument Registration);
}
