using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services;
using PaceLetics.Web.Services.Calendar;
using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Tests;

public sealed class TrainingCalendarServiceTests
{
    [Fact]
    public async Task GetCalendarItemsForAthlete_ReturnsTrainingCourseDatesAndEventsChronologically()
    {
        var course = CreateCourse("course-1", "Laufschule", "plan-1");
        course.Dates.Add(new CourseDateDocument
        {
            Id = "date-1",
            Title = "Techniktraining",
            StartsAt = new DateTime(2026, 7, 2, 18, 0, 0),
            EndsAt = new DateTime(2026, 7, 2, 19, 0, 0),
            Location = "Stadion"
        });

        var courseService = new FakeCourseService(course);
        courseService.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = course.Id,
            Title = "Startanalyse",
            StartsAt = new DateTime(2026, 7, 3, 17, 0, 0),
            EndsAt = new DateTime(2026, 7, 3, 18, 0, 0)
        });

        var trainingPlan = new TrainingPlan(
            "plan-1",
            "Level 1",
            new[]
            {
                CreateSession(
                    "session-1",
                    "Easy Run",
                    new DateTime(2026, 7, 1),
                    new TrainingSessionAppointment(
                        new DateTime(2026, 7, 1, 18, 30, 0),
                        new DateTime(2026, 7, 1, 19, 15, 0),
                        "Track"))
            });
        var service = new TrainingCalendarService(courseService, new FakeTrainingPlanService(trainingPlan));

        var items = await service.GetCalendarItemsForAthleteAsync("athlete-1");

        Assert.Collection(
            items,
            item =>
            {
                Assert.Equal(TrainingCalendarItemKinds.TrainingSession, item.Kind);
                Assert.Equal("Easy Run", item.Title);
                Assert.Equal("Level 1", item.PlanName);
                Assert.Equal("Laufschule", item.CourseName);
                Assert.Equal(new DateTime(2026, 7, 1, 18, 30, 0), item.StartsAt);
            },
            item =>
            {
                Assert.Equal(TrainingCalendarItemKinds.CourseDate, item.Kind);
                Assert.Equal("Techniktraining", item.Title);
                Assert.Equal("Stadion", item.Location);
            },
            item =>
            {
                Assert.Equal(TrainingCalendarItemKinds.CourseEvent, item.Kind);
                Assert.Equal("Startanalyse", item.Title);
            });
    }

    [Fact]
    public async Task GetCalendarItemsForAthlete_MarksRegisteredEvents()
    {
        var course = CreateCourse("course-1", "Laufschule", "plan-1");
        var courseService = new FakeCourseService(course);
        courseService.Events.Add(new CourseEventDocument
        {
            Id = "event-1",
            CourseId = course.Id,
            Title = "Startanalyse",
            StartsAt = new DateTime(2026, 7, 3, 17, 0, 0),
            EndsAt = new DateTime(2026, 7, 3, 18, 0, 0)
        });
        courseService.Registrations.Add(new CourseEventRegistrationDocument
        {
            CourseId = course.Id,
            EventId = "event-1",
            AthleteUserId = "athlete-1",
            Status = CourseEventRegistrationStatus.Registered
        });
        var service = new TrainingCalendarService(
            courseService,
            new FakeTrainingPlanService(new TrainingPlan("plan-1", "Level 1", new[] { CreateSession("session-1", "Run", new DateTime(2026, 7, 1)) })));

        var items = await service.GetCalendarItemsForAthleteAsync("athlete-1");

        var courseEvent = Assert.Single(items, item => item.Kind == TrainingCalendarItemKinds.CourseEvent);
        Assert.True(courseEvent.IsRegistered);
    }

    private static CourseDocument CreateCourse(string courseId, string name, string planId)
    {
        return new CourseDocument
        {
            Id = courseId,
            CourseId = courseId,
            Name = name,
            IsPublished = true,
            TrainingPlanPublications =
            {
                new CourseTrainingPlanPublicationDocument
                {
                    TrainingPlanId = planId,
                    PublishedAt = DateTime.UtcNow.AddDays(-1)
                }
            }
        };
    }

    private static TrainingSession CreateSession(
        string id,
        string name,
        DateTime date,
        TrainingSessionAppointment? appointment = null)
    {
        return new TrainingSession(
            id,
            name,
            date,
            Array.Empty<RunningSession>(),
            new[] { new WorkoutSessionDefinition("workout-1", "Workout") },
            Array.Empty<TrainingSessionActivity>(),
            Array.Empty<TrainingSessionActivity>(),
            TrainingEffect.Empty,
            appointment ?? TrainingSessionAppointment.Empty);
    }

    private sealed class FakeTrainingPlanService : ITrainingPlanService
    {
        private readonly IReadOnlyList<TrainingPlan> _plans;

        public FakeTrainingPlanService(params TrainingPlan[] plans)
        {
            _plans = plans;
        }

        public IReadOnlyList<TrainingPlan> LoadTrainingPlans()
        {
            return _plans;
        }

        public Task<IReadOnlyList<TrainingPlan>> LoadTrainingPlansForUserAsync(string? userId)
        {
            return Task.FromResult(string.IsNullOrWhiteSpace(userId) ? Array.Empty<TrainingPlan>() : _plans);
        }

        public IReadOnlyList<RunningSession> LoadLegacySessions()
        {
            return Array.Empty<RunningSession>();
        }

        public IReadOnlyList<PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingPlanDefinition> LoadTrainingPlanDefinitions()
        {
            return Array.Empty<PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingPlanDefinition>();
        }

        public TrainingPlan CreateTrainingPlan(string name)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan UpdateTrainingPlan(string planId, string name)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan AddTrainingSession(string planId, PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingSessionDefinition session)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan UpdateTrainingSession(string planId, string sessionId, PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingSessionDefinition session)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan RemoveTrainingSession(string planId, string sessionId)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan AddTrainingPlanBlock(string planId, PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingPlanBlockDefinition block)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan UpdateTrainingPlanBlock(string planId, string blockId, PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingPlanBlockDefinition block)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan RemoveTrainingPlanBlock(string planId, string blockId)
        {
            throw new NotSupportedException();
        }

        public void SaveTrainingPlanBlocks(string planId, IEnumerable<PaceLetics.TrainingPlanModule.CodeBase.Definitions.TrainingPlanBlockDefinition> blocks)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeCourseService : ICourseService
    {
        private readonly IReadOnlyList<CourseDocument> _joinedCourses;

        public FakeCourseService(params CourseDocument[] joinedCourses)
        {
            _joinedCourses = joinedCourses;
        }

        public List<CourseEventDocument> Events { get; } = new();
        public List<CourseEventRegistrationDocument> Registrations { get; } = new();

        public Task<IReadOnlyList<CourseDocument>> GetJoinedCoursesAsync(string athleteUserId)
        {
            return Task.FromResult(string.IsNullOrWhiteSpace(athleteUserId) ? Array.Empty<CourseDocument>() : _joinedCourses);
        }

        public Task<IReadOnlyList<CourseEventDocument>> GetEventsAsync(string courseId)
        {
            return Task.FromResult<IReadOnlyList<CourseEventDocument>>(Events.Where(courseEvent => courseEvent.CourseId == courseId).ToList());
        }

        public Task<CourseEventRegistrationDocument?> GetEventRegistrationForAthleteAsync(string courseId, string eventId, string athleteUserId)
        {
            return Task.FromResult(Registrations.FirstOrDefault(registration =>
                registration.CourseId == courseId
                && registration.EventId == eventId
                && registration.AthleteUserId == athleteUserId));
        }

        public Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsForAthleteAsync(string athleteUserId)
        {
            return Task.FromResult<IReadOnlyList<CourseEventRegistrationDocument>>(
                Registrations.Where(registration => registration.AthleteUserId == athleteUserId).ToList());
        }

        public Task<IReadOnlyList<CourseOverview>> GetCoursesForAthleteAsync(string athleteUserId) => throw new NotSupportedException();
        public Task<IReadOnlyList<CourseDocument>> GetCoursesForTrainerAsync(string trainerUserId) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> GetPublishedTrainingPlanIdsForAthleteAsync(string athleteUserId) => throw new NotSupportedException();
        public Task<CourseDocument> CreateCourseAsync(CourseCreateRequest request, string creatorTrainerUserId, string creatorDisplayName) => throw new NotSupportedException();
        public Task UpdateCourseVisibilityAsync(string courseId, FeedTarget visibilityTarget, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task DeleteCourseAsync(string courseId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<CourseEnrollmentDocument> JoinCourseAsync(string courseId, string athleteUserId) => throw new NotSupportedException();
        public Task<CourseEnrollmentDocument> LeaveCourseAsync(string courseId, string athleteUserId) => throw new NotSupportedException();
        public Task AssignTrainerAsync(string courseId, string trainerUserId, string displayName) => throw new NotSupportedException();
        public Task AddTrainerAsync(string courseId, string trainerUserId, string displayName, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task RemoveTrainerAsync(string courseId, string trainerUserId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<CourseDateDocument> AddCourseDateAsync(string courseId, string title, DateTime startsAt, DateTime endsAt, string requestingTrainerUserId, string location = "", string notes = "") => throw new NotSupportedException();
        public Task RemoveCourseDateAsync(string courseId, string dateId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task PublishTrainingPlanAsync(string courseId, string trainingPlanId, string publishedByUserId, DateTime? visibleFrom = null, FeedTarget? target = null) => throw new NotSupportedException();
        public Task RemoveTrainingPlanPublicationAsync(string courseId, string trainingPlanId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<CourseChallengeDocument> CreateChallengeAsync(string courseId, CourseChallengeCreateRequest request, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task RemoveChallengeAsync(string courseId, string challengeId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<IReadOnlyList<CourseChallengeDocument>> GetChallengesForAthleteAsync(string athleteUserId) => throw new NotSupportedException();
        public Task<CourseEventDocument> CreateEventAsync(string courseId, string title, DateTime startsAt, DateTime endsAt, string createdByUserId, string description = "", string location = "", int? capacity = null, DateTime? registrationDeadline = null, string eventType = CourseEventTypes.General) => throw new NotSupportedException();
        public Task DeleteEventAsync(string courseId, string eventId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsForTrainerAsync(string courseId, string eventId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<CourseEventRegistrationDocument> RegisterForEventAsync(string courseId, string eventId, string athleteUserId) => throw new NotSupportedException();
        public Task<CourseEventRegistrationDocument> CancelEventRegistrationAsync(string courseId, string eventId, string athleteUserId) => throw new NotSupportedException();
    }
}
