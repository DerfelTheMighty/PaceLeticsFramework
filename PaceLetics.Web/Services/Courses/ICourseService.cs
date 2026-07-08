using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Services.Courses;

public interface ICourseService
{
    Task<IReadOnlyList<CourseOverview>> GetCoursesForAthleteAsync(string athleteUserId);

    Task<IReadOnlyList<CourseDocument>> GetCoursesForTrainerAsync(string trainerUserId);

    Task<IReadOnlyList<CourseDocument>> GetJoinedCoursesAsync(string athleteUserId);

    Task<IReadOnlyList<string>> GetPublishedTrainingPlanIdsForAthleteAsync(string athleteUserId);

    Task<CourseDocument> CreateCourseAsync(CourseCreateRequest request, string creatorTrainerUserId, string creatorDisplayName);

    Task UpdateCourseVisibilityAsync(string courseId, FeedTarget visibilityTarget, string requestingTrainerUserId);

    Task DeleteCourseAsync(string courseId, string requestingTrainerUserId);

    Task<CourseEnrollmentDocument> JoinCourseAsync(string courseId, string athleteUserId);

    Task<CourseEnrollmentDocument> LeaveCourseAsync(string courseId, string athleteUserId);

    Task AssignTrainerAsync(string courseId, string trainerUserId, string displayName);

    Task AddTrainerAsync(string courseId, string trainerUserId, string displayName, string requestingTrainerUserId);

    Task RemoveTrainerAsync(string courseId, string trainerUserId, string requestingTrainerUserId);

    Task<CourseDateDocument> AddCourseDateAsync(
        string courseId,
        string title,
        DateTime startsAt,
        DateTime endsAt,
        string requestingTrainerUserId,
        string location = "",
        string notes = "");

    Task RemoveCourseDateAsync(string courseId, string dateId, string requestingTrainerUserId);

    Task PublishTrainingPlanAsync(string courseId, string trainingPlanId, string publishedByUserId, DateTime? visibleFrom = null, FeedTarget? target = null);

    Task RemoveTrainingPlanPublicationAsync(string courseId, string trainingPlanId, string requestingTrainerUserId);

    Task<CourseChallengeDocument> CreateChallengeAsync(
        string courseId,
        CourseChallengeCreateRequest request,
        string requestingTrainerUserId);

    Task RemoveChallengeAsync(string courseId, string challengeId, string requestingTrainerUserId);

    Task<IReadOnlyList<CourseChallengeDocument>> GetChallengesForAthleteAsync(string athleteUserId);

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
        DateTime? registrationDeadline = null,
        string eventType = CourseEventTypes.General);

    Task DeleteEventAsync(string courseId, string eventId, string requestingTrainerUserId);

    Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsForTrainerAsync(
        string courseId,
        string eventId,
        string requestingTrainerUserId);

    Task<CourseEventRegistrationDocument?> GetEventRegistrationForAthleteAsync(
        string courseId,
        string eventId,
        string athleteUserId);

    Task<CourseEventRegistrationDocument> RegisterForEventAsync(string courseId, string eventId, string athleteUserId);

    Task<CourseEventRegistrationDocument> CancelEventRegistrationAsync(string courseId, string eventId, string athleteUserId);
}
