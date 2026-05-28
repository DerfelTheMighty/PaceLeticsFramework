using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Tests;

public sealed class CourseServiceTests
{
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
            Events.RemoveAll(existing => existing.Id == courseEvent.Id);
            Events.Add(courseEvent);
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
}
