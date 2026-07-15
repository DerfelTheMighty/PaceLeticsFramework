using Microsoft.Extensions.Localization;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Web.Services.Courses;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IGroupService _groupService;
    private readonly IStringLocalizer<CourseService>? _localizer;
    private readonly ICourseRunningAnalysisRegistrationAdapter? _runningAnalysisRegistrationAdapter;
    private readonly TimeProvider _timeProvider;

    public CourseService(
        ICourseRepository repository,
        IGroupService? groupService = null,
        IStringLocalizer<CourseService>? localizer = null,
        ICourseRunningAnalysisRegistrationAdapter? runningAnalysisRegistrationAdapter = null,
        TimeProvider? timeProvider = null)
    {
        _repository = repository;
        _groupService = groupService ?? NullGroupService.Instance;
        _localizer = localizer;
        _runningAnalysisRegistrationAdapter = runningAnalysisRegistrationAdapter;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<IReadOnlyList<CourseOverview>> GetCoursesForAthleteAsync(string athleteUserId)
    {
        var courses = await _repository.GetCoursesAsync();
        var enrollments = await _repository.GetEnrollmentsForAthleteAsync(athleteUserId);
        var activeGroupIds = await _groupService.GetActiveGroupIdsForAthleteAsync(athleteUserId);
        var activeGroupIdSet = activeGroupIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var enrollmentByCourse = enrollments
            .GroupBy(enrollment => enrollment.CourseId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(enrollment => enrollment.RegisteredAt).First());

        return courses
            .Where(course => course.IsPublished)
            .Where(course => IsVisibleForAthlete(course, activeGroupIdSet)
                             || enrollmentByCourse.TryGetValue(course.Id, out var enrollment)
                             && enrollment.Status == CourseEnrollmentStatus.Active)
            .Select(course =>
            {
                enrollmentByCourse.TryGetValue(course.Id, out var enrollment);
                return new CourseOverview(course, enrollment);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CourseDocument>> GetCoursesForTrainerAsync(string trainerUserId)
    {
        if (string.IsNullOrWhiteSpace(trainerUserId))
            return Array.Empty<CourseDocument>();

        var courses = await _repository.GetCoursesAsync();

        return courses
            .Where(course => course.Trainers.Any(trainer => trainer.TrainerUserId == trainerUserId))
            .OrderBy(course => course.StartDate)
            .ThenBy(course => course.Name)
            .ToList();
    }

    public async Task<IReadOnlyList<CourseDocument>> GetJoinedCoursesAsync(string athleteUserId)
    {
        var courses = await GetCoursesForAthleteAsync(athleteUserId);
        return courses
            .Where(course => course.IsJoined)
            .Select(course => course.Course)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetPublishedTrainingPlanIdsForAthleteAsync(string athleteUserId)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var joinedCourses = await GetJoinedCoursesAsync(athleteUserId);
        var planIds = await _groupService.GetVisibleTrainingPlanIdsForAthleteAsync(athleteUserId, joinedCourses);
        var legacyPlanIds = joinedCourses
            .SelectMany(course => course.TrainingPlanPublications.Select(publication => new
            {
                CourseId = course.Id,
                Publication = publication
            }))
            .Where(item => item.Publication.IsVisibleInCourse(item.CourseId, now))
            .Select(item => item.Publication.TrainingPlanId)
            .Where(planId => !string.IsNullOrWhiteSpace(planId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return planIds
            .Concat(legacyPlanIds)
            .Where(planId => !string.IsNullOrWhiteSpace(planId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<CourseDocument> CreateCourseAsync(
        CourseCreateRequest request,
        string creatorTrainerUserId,
        string creatorDisplayName)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(creatorTrainerUserId))
            throw new InvalidOperationException(Text("CreateCourseTrainerRequired", "A course needs a signed-in coach."));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException(Text("CreateCourseNameRequired", "A course needs a name."));

        if (request.EndDate.Date < request.StartDate.Date)
            throw new InvalidOperationException(Text("CreateCourseEndBeforeStart", "The course end date cannot be before the start date."));

        var courseId = CreateCourseId(request.Name);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var course = new CourseDocument
        {
            Id = courseId,
            CourseId = courseId,
            Slug = courseId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Level = CourseLevelFormatting.Format(request.Level),
            OrganizationId = request.OrganizationId?.Trim() ?? string.Empty,
            TeamId = string.IsNullOrWhiteSpace(request.TeamId) ? courseId : request.TeamId.Trim(),
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            CreatedByTrainerUserId = creatorTrainerUserId,
            CreatedAt = now,
            IsPublished = request.IsPublished,
            VisibilityTarget = ResolveCourseVisibilityTarget(request.VisibilityTarget),
            Trainers =
            {
                new CourseTrainerDocument
                {
                    TrainerUserId = creatorTrainerUserId,
                    DisplayName = NormalizeDisplayName(creatorDisplayName, creatorTrainerUserId),
                    Role = Text("RoleCourseLead", "Course lead"),
                    CanManagePlans = true,
                    CanManageEvents = true,
                    CanManageMembers = true
                }
            }
        };

        await _repository.UpsertCourseAsync(course);
        return course;
    }

    public Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsForAthleteAsync(string athleteUserId)
    {
        return _repository.GetEventRegistrationsForAthleteAsync(athleteUserId);
    }

    public async Task UpdateCourseVisibilityAsync(string courseId, FeedTarget visibilityTarget, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireTrainerManagement(course, requestingTrainerUserId);
        course.VisibilityTarget = ResolveCourseVisibilityTarget(visibilityTarget);

        await _repository.UpsertCourseAsync(course);
    }

    public async Task DeleteCourseAsync(string courseId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        if (course.CreatedByTrainerUserId != requestingTrainerUserId)
            throw new InvalidOperationException(Text("DeleteCourseCreatorOnly", "Only the coach who created this course can delete it."));

        await _repository.DeleteCourseAsync(course.Id);
    }

    public async Task<CourseEnrollmentDocument> JoinCourseAsync(string courseId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("JoinCourseAthleteRequired", "Joining a course requires a signed-in athlete."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        var existing = await _repository.GetEnrollmentAsync(course.Id, athleteUserId);
        var enrollment = existing ?? new CourseEnrollmentDocument
        {
            CourseId = course.Id,
            AthleteUserId = athleteUserId,
            RegisteredAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        enrollment.Status = CourseEnrollmentStatus.Active;
        enrollment.CancelledAt = null;

        if (enrollment.RegisteredAt == default)
            enrollment.RegisteredAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _repository.UpsertEnrollmentAsync(enrollment);
        return enrollment;
    }

    public async Task<CourseEnrollmentDocument> LeaveCourseAsync(string courseId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("LeaveCourseAthleteRequired", "Leaving a course requires a signed-in athlete."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        var enrollment = await _repository.GetEnrollmentAsync(course.Id, athleteUserId)
            ?? throw new InvalidOperationException(Text("LeaveCourseNotJoined", "You have not joined this course."));

        var cancelledAt = _timeProvider.GetUtcNow().UtcDateTime;
        enrollment.Status = CourseEnrollmentStatus.Cancelled;
        enrollment.CancelledAt = cancelledAt;

        await _repository.UpsertEnrollmentAsync(enrollment);
        await CancelActiveEventRegistrationsAsync(course.Id, athleteUserId, cancelledAt);

        return enrollment;
    }

    public async Task AssignTrainerAsync(string courseId, string trainerUserId, string displayName)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        course.Trainers.RemoveAll(trainer => trainer.TrainerUserId == trainerUserId);
        course.Trainers.Add(new CourseTrainerDocument
        {
            TrainerUserId = trainerUserId,
            DisplayName = displayName
        });

        await _repository.UpsertCourseAsync(course);
    }

    public async Task AddTrainerAsync(
        string courseId,
        string trainerUserId,
        string displayName,
        string requestingTrainerUserId)
    {
        if (string.IsNullOrWhiteSpace(trainerUserId))
            throw new InvalidOperationException(Text("AddTrainerRequired", "Please select a coach."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireTrainerManagement(course, requestingTrainerUserId);

        course.Trainers.RemoveAll(trainer => trainer.TrainerUserId == trainerUserId);
        course.Trainers.Add(new CourseTrainerDocument
        {
            TrainerUserId = trainerUserId,
            DisplayName = NormalizeDisplayName(displayName, trainerUserId),
            Role = Text("RoleTrainer", "Coach"),
            CanManagePlans = true,
            CanManageEvents = true,
            CanManageMembers = true
        });

        await _repository.UpsertCourseAsync(course);
    }

    public async Task RemoveTrainerAsync(string courseId, string trainerUserId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        if (course.CreatedByTrainerUserId == trainerUserId)
            throw new InvalidOperationException(Text("RemoveTrainerLeadCannotLeave", "The course lead can delete the course, but cannot leave it."));

        if (trainerUserId != requestingTrainerUserId)
            RequireTrainerManagement(course, requestingTrainerUserId);

        var removed = course.Trainers.RemoveAll(trainer => trainer.TrainerUserId == trainerUserId);
        if (removed == 0)
            throw new InvalidOperationException(Text("RemoveTrainerNotAssigned", "This coach is not assigned to the course."));

        await _repository.UpsertCourseAsync(course);
    }

    public async Task<CourseDateDocument> AddCourseDateAsync(
        string courseId,
        string title,
        DateTime startsAt,
        DateTime endsAt,
        string requestingTrainerUserId,
        string location = "",
        string notes = "")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException(Text("DateTitleRequired", "A date needs a title."));

        if (endsAt <= startsAt)
            throw new InvalidOperationException(Text("DateEndAfterStart", "The date end time must be after the start time."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, requestingTrainerUserId);

        var date = new CourseDateDocument
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = title.Trim(),
            StartsAt = startsAt,
            EndsAt = endsAt,
            Location = location?.Trim() ?? string.Empty,
            Notes = notes?.Trim() ?? string.Empty
        };

        course.Dates.Add(date);
        course.Dates = course.Dates.OrderBy(date => date.StartsAt).ToList();

        await _repository.UpsertCourseAsync(course);
        return date;
    }

    public async Task RemoveCourseDateAsync(string courseId, string dateId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, requestingTrainerUserId);

        var removed = course.Dates.RemoveAll(date => date.Id == dateId);
        if (removed == 0)
            throw new InvalidOperationException(Text("DateNotFound", "The date was not found."));

        await _repository.UpsertCourseAsync(course);
    }

    public async Task PublishTrainingPlanAsync(
        string courseId,
        string trainingPlanId,
        string publishedByUserId,
        DateTime? visibleFrom = null,
        FeedTarget? target = null)
    {
        if (string.IsNullOrWhiteSpace(trainingPlanId))
            throw new InvalidOperationException(Text("TrainingPlanRequired", "Please select a training plan."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequirePlanManagement(course, publishedByUserId);

        var resolvedTarget = target is null || target.IsEmpty
            ? FeedTarget.Course(course.Id)
            : target.NormalizeCopy();

        if (!string.Equals(resolvedTarget.TargetType, FeedTargetTypes.Course, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(resolvedTarget.TargetId, course.Id, StringComparison.OrdinalIgnoreCase))
        {
            await _groupService.UpsertTrainingPlanPublicationAsync(trainingPlanId, resolvedTarget, publishedByUserId, visibleFrom);
            return;
        }

        var existing = course.TrainingPlanPublications
            .FirstOrDefault(publication => publication.TrainingPlanId == trainingPlanId);
        var courseTarget = FeedTarget.Course(course.Id);

        if (existing is null)
        {
            course.TrainingPlanPublications.Add(new CourseTrainingPlanPublicationDocument
            {
                TrainingPlanId = trainingPlanId,
                PublishedAt = _timeProvider.GetUtcNow().UtcDateTime,
                PublishedByUserId = publishedByUserId,
                VisibleFrom = visibleFrom,
                Target = courseTarget
            });
        }
        else
        {
            existing.VisibleFrom = visibleFrom;
            existing.Target = courseTarget;
            existing.PublishedByUserId = string.IsNullOrWhiteSpace(existing.PublishedByUserId)
                ? publishedByUserId
                : existing.PublishedByUserId;
        }

        await _repository.UpsertCourseAsync(course);
    }

    public async Task RemoveTrainingPlanPublicationAsync(string courseId, string trainingPlanId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequirePlanManagement(course, requestingTrainerUserId);

        var removed = course.TrainingPlanPublications.RemoveAll(publication => publication.TrainingPlanId == trainingPlanId);
        if (removed == 0)
            throw new InvalidOperationException(Text("TrainingPlanNotFound", "The training plan was not found."));

        await _repository.UpsertCourseAsync(course);
    }

    public async Task<CourseChallengeDocument> CreateChallengeAsync(
        string courseId,
        CourseChallengeCreateRequest request,
        string requestingTrainerUserId)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidOperationException(Text("ChallengeTitleRequired", "A challenge needs a title."));

        if (request.EndsAt.Date < request.StartsAt.Date)
            throw new InvalidOperationException(Text("ChallengeEndBeforeStart", "The challenge end date cannot be before the start date."));

        if (request.TargetValue is < 0)
            throw new InvalidOperationException(Text("ChallengeTargetPositive", "The challenge target cannot be negative."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, requestingTrainerUserId);
        course.Challenges ??= new List<CourseChallengeDocument>();

        var challenge = new CourseChallengeDocument
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ChallengeType = NormalizeChallengeType(request.ChallengeType),
            StartsAt = request.StartsAt.Date,
            EndsAt = request.EndsAt.Date,
            TargetValue = request.TargetValue,
            Unit = request.Unit?.Trim() ?? string.Empty,
            CreatedByUserId = requestingTrainerUserId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsPublished = request.IsPublished
        };

        course.Challenges.Add(challenge);
        course.Challenges = course.Challenges
            .OrderBy(existing => existing.StartsAt)
            .ThenBy(existing => existing.Title)
            .ToList();

        await _repository.UpsertCourseAsync(course);
        return challenge;
    }

    public async Task RemoveChallengeAsync(string courseId, string challengeId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, requestingTrainerUserId);
        course.Challenges ??= new List<CourseChallengeDocument>();

        var removed = course.Challenges.RemoveAll(challenge => challenge.Id == challengeId);
        if (removed == 0)
            throw new InvalidOperationException(Text("ChallengeNotFound", "The challenge was not found."));

        await _repository.UpsertCourseAsync(course);
    }

    public async Task<IReadOnlyList<CourseChallengeDocument>> GetChallengesForAthleteAsync(string athleteUserId)
    {
        var today = _timeProvider.GetUtcNow().UtcDateTime.Date;
        var joinedCourses = await GetJoinedCoursesAsync(athleteUserId);

        return joinedCourses
            .SelectMany(course => course.Challenges ?? Enumerable.Empty<CourseChallengeDocument>())
            .Where(challenge => challenge.IsPublished && challenge.EndsAt.Date >= today)
            .OrderBy(challenge => challenge.StartsAt)
            .ThenBy(challenge => challenge.Title)
            .ToList();
    }

    public Task<IReadOnlyList<CourseEventDocument>> GetEventsAsync(string courseId)
    {
        return _repository.GetEventsAsync(courseId);
    }

    public async Task<CourseEventDocument> CreateEventAsync(
        string courseId,
        string title,
        DateTime startsAt,
        DateTime endsAt,
        string createdByUserId,
        string description = "",
        string location = "",
        int? capacity = null,
        DateTime? registrationDeadline = null,
        string eventType = CourseEventTypes.General)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException(Text("EventTitleRequired", "An event needs a title."));

        if (endsAt <= startsAt)
            throw new InvalidOperationException(Text("EventEndAfterStart", "The event end time must be after the start time."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, createdByUserId);

        var courseEvent = new CourseEventDocument
        {
            CourseId = course.Id,
            Title = title,
            StartsAt = startsAt,
            EndsAt = endsAt,
            EventType = string.IsNullOrWhiteSpace(eventType) ? CourseEventTypes.General : eventType,
            Description = description,
            Location = location,
            Capacity = capacity,
            RegistrationDeadline = registrationDeadline,
            CreatedByUserId = createdByUserId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _repository.UpsertEventAsync(courseEvent);
        return courseEvent;
    }

    public async Task DeleteEventAsync(string courseId, string eventId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, requestingTrainerUserId);

        var courseEvent = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "The event was not found."));

        await _repository.DeleteEventAsync(course.Id, courseEvent.Id);
    }

    public async Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsForTrainerAsync(
        string courseId,
        string eventId,
        string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        RequireEventManagement(course, requestingTrainerUserId);

        _ = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "The event was not found."));

        return (await _repository.GetEventRegistrationsAsync(courseId, eventId))
            .Where(registration => registration.Status == CourseEventRegistrationStatus.Registered)
            .OrderBy(registration => registration.RegisteredAt)
            .ToList();
    }

    public async Task<CourseEventRegistrationDocument?> GetEventRegistrationForAthleteAsync(
        string courseId,
        string eventId,
        string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return null;

        _ = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "The event was not found."));

        return await _repository.GetEventRegistrationAsync(courseId, eventId, athleteUserId);
    }

    public async Task<CourseEventRegistrationDocument> RegisterForEventAsync(string courseId, string eventId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("RegisterEventAthleteRequired", "Event registration requires a signed-in athlete."));

        var enrollment = await _repository.GetEnrollmentAsync(courseId, athleteUserId);
        if (enrollment?.Status != CourseEnrollmentStatus.Active)
            throw new InvalidOperationException(Text("RegisterEventRequiresEnrollment", "You must join the course before registering for events."));

        var courseEvent = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "The event was not found."));

        if (courseEvent.RegistrationDeadline is not null && courseEvent.RegistrationDeadline < _timeProvider.GetUtcNow().UtcDateTime)
            throw new InvalidOperationException(Text("RegisterEventDeadlinePassed", "The registration deadline for this event has passed."));

        var registrations = await _repository.GetEventRegistrationsAsync(courseId, eventId);
        var activeRegistrations = registrations.Count(registration =>
            registration.Status == CourseEventRegistrationStatus.Registered);

        if (courseEvent.Capacity is not null && activeRegistrations >= courseEvent.Capacity)
        {
            var existing = registrations.FirstOrDefault(registration => registration.AthleteUserId == athleteUserId);
            if (existing?.Status != CourseEventRegistrationStatus.Registered)
                throw new InvalidOperationException(Text("RegisterEventFull", "No spots are available for this event."));
        }

        var registration = await _repository.GetEventRegistrationAsync(courseId, eventId, athleteUserId)
            ?? new CourseEventRegistrationDocument
            {
                CourseId = courseId,
                EventId = eventId,
                AthleteUserId = athleteUserId,
                RegisteredAt = _timeProvider.GetUtcNow().UtcDateTime
            };

        registration.Status = CourseEventRegistrationStatus.Registered;
        registration.CancelledAt = null;

        if (registration.RegisteredAt == default)
            registration.RegisteredAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _repository.UpsertEventRegistrationAsync(registration);
        await NotifyRunningAnalysisRegistrationAsync(courseEvent, registration);
        return registration;
    }

    public async Task<CourseEventRegistrationDocument> CancelEventRegistrationAsync(string courseId, string eventId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("CancelEventAthleteRequired", "Cancelling an event registration requires a signed-in athlete."));

        _ = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "The event was not found."));

        var registration = await _repository.GetEventRegistrationAsync(courseId, eventId, athleteUserId)
            ?? throw new InvalidOperationException(Text("CancelEventNotRegistered", "You are not registered for this event."));

        if (registration.Status != CourseEventRegistrationStatus.Registered)
            throw new InvalidOperationException(Text("CancelEventNotRegistered", "You are not registered for this event."));

        registration.Status = CourseEventRegistrationStatus.Cancelled;
        registration.CancelledAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _repository.UpsertEventRegistrationAsync(registration);
        return registration;
    }

    private async Task CancelActiveEventRegistrationsAsync(string courseId, string athleteUserId, DateTime cancelledAt)
    {
        var events = await _repository.GetEventsAsync(courseId);

        foreach (var courseEvent in events)
        {
            var registration = await _repository.GetEventRegistrationAsync(courseId, courseEvent.Id, athleteUserId);
            if (registration?.Status != CourseEventRegistrationStatus.Registered)
                continue;

            registration.Status = CourseEventRegistrationStatus.Cancelled;
            registration.CancelledAt = cancelledAt;
            await _repository.UpsertEventRegistrationAsync(registration);
        }
    }

    private async Task NotifyRunningAnalysisRegistrationAsync(
        CourseEventDocument courseEvent,
        CourseEventRegistrationDocument registration)
    {
        if (_runningAnalysisRegistrationAdapter is null)
            return;

        if (!string.Equals(courseEvent.EventType, CourseEventTypes.RunningAnalysis, StringComparison.OrdinalIgnoreCase))
            return;

        var course = await _repository.GetCourseAsync(courseEvent.CourseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "The course was not found."));

        await _runningAnalysisRegistrationAdapter.OnRegisteredAsync(course, courseEvent, registration);
    }

    private static string CreateCourseId(string name)
    {
        var slug = new string(name
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());
        slug = string.Join("-", slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(slug))
            slug = "kurs";

        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{slug}-{suffix}";
    }

    private static string NormalizeDisplayName(string displayName, string fallbackUserId)
    {
        return string.IsNullOrWhiteSpace(displayName)
            ? fallbackUserId
            : displayName.Trim();
    }

    private static string NormalizeChallengeType(string? challengeType)
    {
        if (string.Equals(challengeType, CourseChallengeTypes.Attendance, StringComparison.OrdinalIgnoreCase))
            return CourseChallengeTypes.Attendance;

        if (string.Equals(challengeType, CourseChallengeTypes.Distance, StringComparison.OrdinalIgnoreCase))
            return CourseChallengeTypes.Distance;

        return CourseChallengeTypes.General;
    }

    private static FeedTarget ResolveCourseVisibilityTarget(FeedTarget? target)
    {
        if (target is null || target.IsEmpty || target.IsGlobal)
            return FeedTarget.Global();

        var normalized = target.NormalizeCopy();
        if (string.Equals(normalized.TargetType, FeedTargetTypes.Group, StringComparison.OrdinalIgnoreCase))
            return normalized;

        return FeedTarget.Global();
    }

    private static bool IsVisibleForAthlete(CourseDocument course, IReadOnlySet<string> activeGroupIds)
    {
        var target = ResolveCourseVisibilityTarget(course.VisibilityTarget);
        if (target.IsGlobal)
            return true;

        return string.Equals(target.TargetType, FeedTargetTypes.Group, StringComparison.OrdinalIgnoreCase)
               && activeGroupIds.Contains(target.TargetId);
    }

    private void RequireTrainerManagement(CourseDocument course, string requestingTrainerUserId)
    {
        var trainer = course.Trainers.FirstOrDefault(trainer => trainer.TrainerUserId == requestingTrainerUserId);
        if (trainer is null || !trainer.CanManageMembers)
            throw new InvalidOperationException(Text("PermissionManageTrainers", "You are not allowed to manage this course's coaches."));
    }

    private void RequireEventManagement(CourseDocument course, string requestingTrainerUserId)
    {
        var trainer = course.Trainers.FirstOrDefault(trainer => trainer.TrainerUserId == requestingTrainerUserId);
        if (trainer is null || !trainer.CanManageEvents)
            throw new InvalidOperationException(Text("PermissionManageEvents", "You are not allowed to manage dates and events for this course."));
    }

    private void RequirePlanManagement(CourseDocument course, string requestingTrainerUserId)
    {
        var trainer = course.Trainers.FirstOrDefault(trainer => trainer.TrainerUserId == requestingTrainerUserId);
        if (trainer is null || !trainer.CanManagePlans)
            throw new InvalidOperationException(Text("PermissionManagePlans", "You are not allowed to manage training plans for this course."));
    }

    private string Text(string key, string fallback)
    {
        if (_localizer is null)
            return fallback;

        var localized = _localizer[key];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }

    private sealed class NullGroupService : IGroupService
    {
        public static NullGroupService Instance { get; } = new();

        public Task<IReadOnlyList<GroupOverview>> GetGroupsForAthleteAsync(string athleteUserId) => Empty<GroupOverview>();
        public Task<IReadOnlyList<GroupDocument>> GetGroupsForTrainerAsync(string trainerUserId) => Empty<GroupDocument>();
        public Task<IReadOnlyList<GroupMembershipDocument>> GetMembershipsForGroupAsync(string groupId, string requestingTrainerUserId) => Empty<GroupMembershipDocument>();
        public Task<IReadOnlyList<string>> GetActiveGroupIdsForAthleteAsync(string athleteUserId) => Empty<string>();
        public Task<GroupDocument?> GetGroupAsync(string groupId) => Task.FromResult<GroupDocument?>(null);
        public Task<GroupDocument> CreateGroupAsync(GroupCreateRequest request, string trainerUserId, string trainerDisplayName) => throw new NotSupportedException();
        public Task<GroupDocument> UpdateGroupAsync(string groupId, GroupCreateRequest request, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task DeleteGroupAsync(string groupId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<GroupMembershipDocument> JoinGroupAsync(string groupId, string athleteUserId) => throw new NotSupportedException();
        public Task<GroupMembershipDocument> LeaveGroupAsync(string groupId, string athleteUserId) => throw new NotSupportedException();
        public Task<GroupMembershipDocument> ApproveMembershipAsync(string groupId, string athleteUserId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<GroupMembershipDocument> RejectMembershipAsync(string groupId, string athleteUserId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<IReadOnlyList<TrainingPlanPublicationDocument>> GetTrainingPlanPublicationsAsync() => Empty<TrainingPlanPublicationDocument>();
        public Task UpsertTrainingPlanPublicationAsync(string trainingPlanId, FeedTarget target, string publishedByUserId, DateTime? visibleFrom = null, DateTime? visibleUntil = null) => throw new NotSupportedException();
        public Task RemoveTrainingPlanPublicationAsync(string publicationId, string requestingTrainerUserId) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> GetVisibleTrainingPlanIdsForAthleteAsync(string athleteUserId, IReadOnlyList<CourseDocument> joinedCourses) => Empty<string>();

        private static Task<IReadOnlyList<T>> Empty<T>()
        {
            return Task.FromResult<IReadOnlyList<T>>(Array.Empty<T>());
        }
    }
}
