namespace PaceLetics.Web.Services.Courses;

public interface ICourseRepository
{
    Task<IReadOnlyList<CourseDocument>> GetCoursesAsync();

    Task<CourseDocument?> GetCourseAsync(string courseId);

    Task UpsertCourseAsync(CourseDocument course);

    Task<IReadOnlyList<CourseEnrollmentDocument>> GetEnrollmentsForAthleteAsync(string athleteUserId);

    Task<IReadOnlyList<CourseEnrollmentDocument>> GetEnrollmentsForCourseAsync(string courseId);

    Task<CourseEnrollmentDocument?> GetEnrollmentAsync(string courseId, string athleteUserId);

    Task UpsertEnrollmentAsync(CourseEnrollmentDocument enrollment);

    Task<IReadOnlyList<CourseEventDocument>> GetEventsAsync(string courseId);

    Task<CourseEventDocument?> GetEventAsync(string courseId, string eventId);

    Task UpsertEventAsync(CourseEventDocument courseEvent);

    Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsAsync(string courseId, string eventId);

    Task<CourseEventRegistrationDocument?> GetEventRegistrationAsync(string courseId, string eventId, string athleteUserId);

    Task UpsertEventRegistrationAsync(CourseEventRegistrationDocument registration);
}
