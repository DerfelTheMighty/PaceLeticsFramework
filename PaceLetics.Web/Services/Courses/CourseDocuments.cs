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

public enum CourseLevel
{
    Level0 = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5
}

public static class CourseLevelFormatting
{
    public static IReadOnlyList<CourseLevel> All { get; } = Enum.GetValues<CourseLevel>();

    public static string Format(CourseLevel level)
    {
        return $"Level {(int)level}";
    }

    public static string Format(string? level)
    {
        return TryParse(level, out var parsed)
            ? Format(parsed)
            : level?.Trim() ?? string.Empty;
    }

    public static bool TryParse(string? level, out CourseLevel parsed)
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(level))
            return false;

        var value = level.Trim();
        if (value.StartsWith("Level", StringComparison.OrdinalIgnoreCase))
            value = value["Level".Length..].Trim();

        if (!int.TryParse(value, out var numericLevel))
            return false;

        if (!Enum.IsDefined(typeof(CourseLevel), numericLevel))
            return false;

        parsed = (CourseLevel)numericLevel;
        return true;
    }
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
    public string CreatedByTrainerUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
    public List<CourseDateDocument> Dates { get; set; } = new();
    public List<CourseTrainerDocument> Trainers { get; set; } = new();
    public List<CourseTrainingPlanPublicationDocument> TrainingPlanPublications { get; set; } = new();
}

public sealed class CourseCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CourseLevel Level { get; set; } = CourseLevel.Level1;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublished { get; set; } = true;
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
