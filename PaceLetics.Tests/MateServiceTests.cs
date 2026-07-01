using AthleteDataAccessLibrary.Contracts;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Definitions;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services;
using PaceLetics.Web.Services.Courses;
using PaceLetics.Web.Services.Mates;

namespace PaceLetics.Tests;

public sealed class MateServiceTests
{
    [Fact]
    public async Task ShareSessionAsync_CreatesAvailabilityForVisibleCoursePlan()
    {
        var fixture = CreateFixture();

        await fixture.CourseService.JoinCourseAsync("course-1", "athlete-1");

        var availability = await fixture.MateService.ShareSessionAsync(
            "athlete-1",
            new MateShareRequest
            {
                CourseId = "course-1",
                PlanId = "plan-1",
                SessionId = "session-1"
            });

        Assert.Equal("course-1", availability.CourseId);
        Assert.Equal("athlete-1", availability.AthleteUserId);
        Assert.Equal("Pitt", availability.AthleteDisplayName);
        Assert.Equal(5000, availability.DistanceMeters);
        Assert.Equal(345, availability.PaceSecondsPerKilometer);
        Assert.True(availability.IsActive);
        Assert.Single(await fixture.MateRepository.GetMateAvailabilitiesForAthleteAsync("athlete-1"));
    }

    [Fact]
    public async Task GetOverviewAsync_ReturnsMatchesOnlyForOwnSharedSessionsInSameCourse()
    {
        var fixture = CreateFixture();
        await fixture.CourseService.JoinCourseAsync("course-1", "athlete-1");
        await fixture.CourseService.JoinCourseAsync("course-1", "athlete-2");

        await fixture.MateRepository.UpsertMateAvailabilityAsync(CreateCandidateAvailability(
            "course-1",
            "athlete-2",
            "Pat",
            350));
        await fixture.MateRepository.UpsertMateAvailabilityAsync(CreateCandidateAvailability(
            "course-2",
            "athlete-3",
            "Other course",
            348));

        var beforeShare = await fixture.MateService.GetOverviewAsync("athlete-1");
        Assert.Empty(beforeShare.Matches);

        await fixture.MateService.ShareSessionAsync(
            "athlete-1",
            new MateShareRequest
            {
                CourseId = "course-1",
                PlanId = "plan-1",
                SessionId = "session-1"
            });

        var overview = await fixture.MateService.GetOverviewAsync("athlete-1");

        var match = Assert.Single(overview.Matches);
        Assert.Equal("athlete-2", match.MateSession.AthleteUserId);
        Assert.Equal("Pat", match.MateSession.AthleteDisplayName);
        Assert.Equal(5, match.PaceDeltaSeconds);
    }

    [Fact]
    public async Task ShareSessionAsync_RequiresJoinedCourse()
    {
        var fixture = CreateFixture();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.MateService.ShareSessionAsync(
                "athlete-1",
                new MateShareRequest
                {
                    CourseId = "course-1",
                    PlanId = "plan-1",
                    SessionId = "session-1"
                }));

        Assert.Empty(await fixture.MateRepository.GetMateAvailabilitiesForAthleteAsync("athlete-1"));
    }

    private static Fixture CreateFixture()
    {
        var plan = CreatePlan("plan-1", DateTime.Today.AddDays(2));
        var courseRepository = new InMemoryCourseRepository(CreateCourse("course-1", "plan-1"), CreateCourse("course-2", "plan-1"));
        var courseService = new CourseService(courseRepository);
        var mateRepository = new InMemoryMateRepository();
        var trainingPlanService = new FakeTrainingPlanService(plan);
        var athleteData = new InMemoryAthleteData(
            CreateAthlete("athlete-1", "Pitt", 345),
            CreateAthlete("athlete-2", "Pat", 350));
        var mateService = new MateService(courseService, mateRepository, trainingPlanService, athleteData);

        return new Fixture(courseService, mateRepository, mateService);
    }

    private static TrainingPlan CreatePlan(string planId, DateTime date)
    {
        var run = new SimpleRunSession(
            "run-1",
            "Easy run",
            date,
            5000,
            PaceKeys.Easy);
        var session = new TrainingSession(
            "session-1",
            "5k easy",
            date,
            new[] { run },
            Array.Empty<WorkoutSessionDefinition>(),
            Array.Empty<TrainingSessionActivity>(),
            Array.Empty<TrainingSessionActivity>(),
            TrainingEffect.Empty,
            new TrainingSessionAppointment(date.AddHours(18), date.AddHours(19), "Track"));

        return new TrainingPlan(planId, "Level 1", new[] { session });
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
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(30),
            CreatedByTrainerUserId = "trainer-1",
            Trainers =
            {
                new CourseTrainerDocument
                {
                    TrainerUserId = "trainer-1",
                    DisplayName = "Coach"
                }
            },
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

    private static AthleteModel CreateAthlete(string userId, string publicUserName, int easyPaceSeconds)
    {
        return new AthleteModel
        {
            Id = userId,
            AthleteId = userId,
            Name = publicUserName,
            PublicProfile = new PublicProfileModel
            {
                PublicUserName = publicUserName,
                NormalizedPublicUserName = publicUserName.ToUpperInvariant()
            },
            PaceModel = new PaceModel
            {
                Easy = TimeSpan.FromSeconds(easyPaceSeconds),
                Marathon = TimeSpan.FromSeconds(easyPaceSeconds - 20),
                Threshold = TimeSpan.FromSeconds(easyPaceSeconds - 40),
                Intervall = TimeSpan.FromSeconds(easyPaceSeconds - 55),
                Repetition = TimeSpan.FromSeconds(easyPaceSeconds - 70)
            }
        };
    }

    private static MateAvailabilityDocument CreateCandidateAvailability(
        string courseId,
        string athleteUserId,
        string displayName,
        int paceSeconds)
    {
        var date = DateTime.Today.AddDays(2);
        return new MateAvailabilityDocument
        {
            Id = MateDocumentIds.Availability(courseId, athleteUserId, "plan-1", "session-1"),
            CourseId = courseId,
            DocumentType = MateDocumentTypes.Availability,
            AthleteUserId = athleteUserId,
            AthleteDisplayName = displayName,
            CourseName = courseId,
            PlanId = "plan-1",
            PlanName = "Level 1",
            SessionId = "session-1",
            SessionName = "5k easy",
            SessionDate = date,
            StartsAt = date.AddHours(18),
            DistanceMeters = 5200,
            PaceKey = PaceKeys.Easy,
            PaceSecondsPerKilometer = paceSeconds,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
    }

    private sealed record Fixture(
        ICourseService CourseService,
        InMemoryMateRepository MateRepository,
        IMateService MateService);

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
            return Task.FromResult(string.IsNullOrWhiteSpace(userId)
                ? Array.Empty<TrainingPlan>()
                : _plans);
        }

        public IReadOnlyList<RunningSession> LoadLegacySessions()
        {
            return Array.Empty<RunningSession>();
        }

        public IReadOnlyList<TrainingPlanDefinition> LoadTrainingPlanDefinitions()
        {
            return Array.Empty<TrainingPlanDefinition>();
        }

        public TrainingPlan CreateTrainingPlan(string name)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan UpdateTrainingPlan(string planId, string name)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan AddTrainingSession(string planId, TrainingSessionDefinition session)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan UpdateTrainingSession(string planId, string sessionId, TrainingSessionDefinition session)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan RemoveTrainingSession(string planId, string sessionId)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan AddTrainingPlanBlock(string planId, TrainingPlanBlockDefinition block)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan UpdateTrainingPlanBlock(string planId, string blockId, TrainingPlanBlockDefinition block)
        {
            throw new NotSupportedException();
        }

        public TrainingPlan RemoveTrainingPlanBlock(string planId, string blockId)
        {
            throw new NotSupportedException();
        }

        public void SaveTrainingPlanBlocks(string planId, IEnumerable<TrainingPlanBlockDefinition> blocks)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class InMemoryAthleteData : IAthleteData
    {
        private readonly List<AthleteModel> _athletes;

        public InMemoryAthleteData(params AthleteModel[] athletes)
        {
            _athletes = athletes.ToList();
        }

        public Task<List<AthleteModel>> GetAthletes()
        {
            return Task.FromResult(_athletes.ToList());
        }

        public Task InsertAthlete(AthleteModel model)
        {
            _athletes.Add(model);
            return Task.CompletedTask;
        }

        public Task DeleteAthlete(string guid)
        {
            _athletes.RemoveAll(athlete => athlete.Id == guid);
            return Task.CompletedTask;
        }

        public Task<AthleteModel?> GetAthlete(string id)
        {
            return Task.FromResult(_athletes.FirstOrDefault(athlete => athlete.Id == id || athlete.AthleteId == id));
        }

        public Task UpdateAthlete(AthleteModel model)
        {
            _athletes.RemoveAll(athlete => athlete.Id == model.Id);
            _athletes.Add(model);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryMateRepository : IMateRepository
    {
        private readonly List<MateAvailabilityDocument> _availabilities = new();

        public Task<IReadOnlyList<MateAvailabilityDocument>> GetMateAvailabilitiesForCourseAsync(string courseId)
        {
            return Task.FromResult<IReadOnlyList<MateAvailabilityDocument>>(
                _availabilities.Where(availability => availability.CourseId == courseId).ToList());
        }

        public Task<IReadOnlyList<MateAvailabilityDocument>> GetMateAvailabilitiesForAthleteAsync(string athleteUserId)
        {
            return Task.FromResult<IReadOnlyList<MateAvailabilityDocument>>(
                _availabilities.Where(availability => availability.AthleteUserId == athleteUserId).ToList());
        }

        public Task<MateAvailabilityDocument?> GetMateAvailabilityAsync(string courseId, string availabilityId)
        {
            return Task.FromResult(_availabilities.FirstOrDefault(availability =>
                availability.CourseId == courseId && availability.Id == availabilityId));
        }

        public Task UpsertMateAvailabilityAsync(MateAvailabilityDocument availability)
        {
            if (string.IsNullOrWhiteSpace(availability.Id))
                availability.Id = MateDocumentIds.Availability(
                    availability.CourseId,
                    availability.AthleteUserId,
                    availability.PlanId,
                    availability.SessionId);

            _availabilities.RemoveAll(existing => existing.Id == availability.Id);
            _availabilities.Add(availability);
            return Task.CompletedTask;
        }

        public Task DeleteMateAvailabilityAsync(string courseId, string availabilityId)
        {
            _availabilities.RemoveAll(availability =>
                availability.CourseId == courseId && availability.Id == availabilityId);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryCourseRepository : ICourseRepository
    {
        private readonly List<CourseDocument> _courses;
        private readonly List<CourseEnrollmentDocument> _enrollments = new();

        public InMemoryCourseRepository(params CourseDocument[] courses)
        {
            _courses = courses.ToList();
        }

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
            _courses.RemoveAll(course => course.Id == courseId);
            _enrollments.RemoveAll(enrollment => enrollment.CourseId == courseId);
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
            return Task.FromResult<IReadOnlyList<CourseEventDocument>>(Array.Empty<CourseEventDocument>());
        }

        public Task<CourseEventDocument?> GetEventAsync(string courseId, string eventId)
        {
            return Task.FromResult<CourseEventDocument?>(null);
        }

        public Task UpsertEventAsync(CourseEventDocument courseEvent)
        {
            return Task.CompletedTask;
        }

        public Task DeleteEventAsync(string courseId, string eventId)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsAsync(string courseId, string eventId)
        {
            return Task.FromResult<IReadOnlyList<CourseEventRegistrationDocument>>(Array.Empty<CourseEventRegistrationDocument>());
        }

        public Task<CourseEventRegistrationDocument?> GetEventRegistrationAsync(string courseId, string eventId, string athleteUserId)
        {
            return Task.FromResult<CourseEventRegistrationDocument?>(null);
        }

        public Task UpsertEventRegistrationAsync(CourseEventRegistrationDocument registration)
        {
            return Task.CompletedTask;
        }
    }
}
