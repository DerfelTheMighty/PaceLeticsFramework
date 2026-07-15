using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Services.Courses;

public static class CourseDocumentTypes
{
    public const string Course = "course";
    public const string Enrollment = "courseEnrollment";
    public const string Event = "courseEvent";
    public const string EventRegistration = "courseEventRegistration";
    public const string MateAvailability = "mateAvailability";
    public const string Group = "group";
    public const string GroupMembership = "groupMembership";
    public const string TrainingPlanPublication = "trainingPlanPublication";
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

public static class CourseEventTypes
{
    public const string General = "general";
    public const string RunningAnalysis = "runningAnalysis";
}

public static class CourseChallengeTypes
{
    public const string General = "general";
    public const string Attendance = "attendance";
    public const string Distance = "distance";
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

public sealed class CourseDocument : IVersionedQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string? ETag { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.Course;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CreatedByTrainerUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
    public FeedTarget VisibilityTarget { get; set; } = FeedTarget.Global();
    public List<CourseDateDocument> Dates { get; set; } = new();
    public List<CourseTrainerDocument> Trainers { get; set; } = new();
    public List<CourseTrainingPlanPublicationDocument> TrainingPlanPublications { get; set; } = new();
    public List<CourseChallengeDocument> Challenges { get; set; } = new();
}

public sealed class CourseCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CourseLevel Level { get; set; } = CourseLevel.Level1;
    public string OrganizationId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublished { get; set; } = true;
    public FeedTarget VisibilityTarget { get; set; } = FeedTarget.Global();
}

public sealed class CourseChallengeCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChallengeType { get; set; } = CourseChallengeTypes.General;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal? TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
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
    public DateTime? VisibleUntil { get; set; }
    public FeedTarget? Target { get; set; }

    public ContentPublication ToContentPublication(string courseId)
    {
        return new ContentPublication
        {
            ContentType = PublishedContentTypes.TrainingPlan,
            ContentId = TrainingPlanId,
            Target = ResolveTarget(courseId),
            PublishedAt = PublishedAt,
            PublishedByUserId = PublishedByUserId,
            VisibleFrom = VisibleFrom,
            VisibleUntil = VisibleUntil
        };
    }

    public bool IsVisibleInCourse(string courseId, DateTime utcNow)
    {
        return ToContentPublication(courseId).IsVisibleFor(FeedTarget.Course(courseId), utcNow);
    }

    private FeedTarget ResolveTarget(string courseId)
    {
        if (Target is null || Target.IsEmpty)
            return FeedTarget.Course(courseId);

        return Target.NormalizeCopy();
    }
}

public sealed class CourseChallengeDocument
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChallengeType { get; set; } = CourseChallengeTypes.General;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal? TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
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

public sealed class CourseEventDocument : IVersionedQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string? ETag { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.Event;
    public string EventType { get; set; } = CourseEventTypes.General;
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

public static class GroupJoinModes
{
    public const string Open = "open";
    public const string ApprovalRequired = "approvalRequired";
}

public static class GroupMembershipStatus
{
    public const string Active = "active";
    public const string Pending = "pending";
    public const string Cancelled = "cancelled";
    public const string Rejected = "rejected";
}

public sealed class GroupDocument : IVersionedQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string? ETag { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.Group;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedByTrainerUserId { get; set; } = string.Empty;
    public string CreatedByDisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
    public string JoinMode { get; set; } = GroupJoinModes.Open;
}

public sealed class GroupCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string JoinMode { get; set; } = GroupJoinModes.Open;
    public bool IsPublished { get; set; } = true;
}

public sealed class GroupMembershipDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = CourseDocumentTypes.GroupMembership;
    public string AthleteUserId { get; set; } = string.Empty;
    public string Status { get; set; } = GroupMembershipStatus.Active;
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string ApprovedByTrainerUserId { get; set; } = string.Empty;
    public DateTime? CancelledAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectedByTrainerUserId { get; set; } = string.Empty;
}

public sealed class GroupOverview
{
    public GroupOverview(GroupDocument group, GroupMembershipDocument? membership)
    {
        Group = group;
        Membership = membership;
    }

    public GroupDocument Group { get; }
    public GroupMembershipDocument? Membership { get; }
    public bool IsJoined => Membership?.Status == GroupMembershipStatus.Active;
    public bool IsPending => Membership?.Status == GroupMembershipStatus.Pending;
}

public sealed class TrainingPlanPublicationDocument : IQueryItem
{
    public const string PartitionKey = "training-plan-publications";

    public string Id { get; set; } = string.Empty;
    public string PublicationPartitionKey { get; set; } = PartitionKey;
    public string DocumentType { get; set; } = CourseDocumentTypes.TrainingPlanPublication;
    public string TrainingPlanId { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string PublishedByUserId { get; set; } = string.Empty;
    public DateTime? VisibleFrom { get; set; }
    public DateTime? VisibleUntil { get; set; }
    public FeedTarget Target { get; set; } = FeedTarget.Global();

    public ContentPublication ToContentPublication()
    {
        return new ContentPublication
        {
            ContentType = PublishedContentTypes.TrainingPlan,
            ContentId = TrainingPlanId,
            Target = Target.NormalizeCopy(),
            PublishedAt = PublishedAt,
            PublishedByUserId = PublishedByUserId,
            VisibleFrom = VisibleFrom,
            VisibleUntil = VisibleUntil
        };
    }
}
