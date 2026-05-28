using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.Courses;

public static class CourseDocumentTypes
{
    public const string Course = "course";
    public const string Enrollment = "courseEnrollment";
    public const string Event = "courseEvent";
    public const string EventRegistration = "courseEventRegistration";
}

public static class CourseEnrollmentStatus
{
    public const string Active = "active";
    public const string Cancelled = "cancelled";
}

public static class CourseEventRegistrationStatus
{
    public const string Registered = "registered";
    public const string Cancelled = "cancelled";
}

public sealed class CourseDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.Course;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublished { get; set; } = true;
    public List<CourseDateDocument> Dates { get; set; } = new();
    public List<CourseTrainerDocument> Trainers { get; set; } = new();
    public List<CourseTrainingPlanPublicationDocument> TrainingPlanPublications { get; set; } = new();
}

public sealed class CourseDateDocument
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public sealed class CourseTrainerDocument
{
    public string TrainerUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "Kursleitung";
    public bool CanManagePlans { get; set; } = true;
    public bool CanManageEvents { get; set; } = true;
    public bool CanManageMembers { get; set; } = true;
}

public sealed class CourseTrainingPlanPublicationDocument
{
    public string TrainingPlanId { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string PublishedByUserId { get; set; } = string.Empty;
    public DateTime? VisibleFrom { get; set; }
}

public sealed class CourseEnrollmentDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.Enrollment;
    public string AthleteUserId { get; set; } = string.Empty;
    public string Status { get; set; } = CourseEnrollmentStatus.Active;
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public sealed class CourseEventDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.Event;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CourseEventRegistrationDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.EventRegistration;
    public string AthleteUserId { get; set; } = string.Empty;
    public string Status { get; set; } = CourseEventRegistrationStatus.Registered;
    public DateTime RegisteredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public sealed class CourseOverview
{
    public CourseOverview(CourseDocument course, CourseEnrollmentDocument? enrollment)
    {
        Course = course;
        Enrollment = enrollment;
    }

    public CourseDocument Course { get; }
    public CourseEnrollmentDocument? Enrollment { get; }
    public bool IsJoined => Enrollment?.Status == CourseEnrollmentStatus.Active;
}
