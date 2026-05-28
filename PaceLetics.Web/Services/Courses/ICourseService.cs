namespace PaceLetics.Web.Services.Courses;

public interface ICourseService
{
    Task<IReadOnlyList<CourseOverview>> GetCoursesForAthleteAsync(string athleteUserId);

    Task<IReadOnlyList<CourseDocument>> GetJoinedCoursesAsync(string athleteUserId);

    Task<IReadOnlyList<string>> GetPublishedTrainingPlanIdsForAthleteAsync(string athleteUserId);

    Task<CourseEnrollmentDocument> JoinCourseAsync(string courseId, string athleteUserId);

    Task<CourseEnrollmentDocument> LeaveCourseAsync(string courseId, string athleteUserId);

    Task AssignTrainerAsync(string courseId, string trainerUserId, string displayName);

    Task PublishTrainingPlanAsync(string courseId, string trainingPlanId, string publishedByUserId, DateTime? visibleFrom = null);

    Task<IReadOnlyList<CourseEventDocument>> GetEventsAsync(string courseId);

    Task<CourseEventDocument> CreateEventAsync(
        string courseId,
        string title,
        DateTime startsAt,
        DateTime endsAt,
        string createdByUserId,
        string description = "",
        string location = "",
        int? capacity = null,
        DateTime? registrationDeadline = null);

    Task<CourseEventRegistrationDocument> RegisterForEventAsync(string courseId, string eventId, string athleteUserId);
}
