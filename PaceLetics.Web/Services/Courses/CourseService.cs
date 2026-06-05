using Microsoft.Extensions.Localization;

namespace PaceLetics.Web.Services.Courses;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IStringLocalizer<CourseService>? _localizer;

    public CourseService(ICourseRepository repository, IStringLocalizer<CourseService>? localizer = null)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<IReadOnlyList<CourseOverview>> GetCoursesForAthleteAsync(string athleteUserId)
    {
        var courses = await _repository.GetCoursesAsync();
        var enrollments = await _repository.GetEnrollmentsForAthleteAsync(athleteUserId);
        var enrollmentByCourse = enrollments
            .GroupBy(enrollment => enrollment.CourseId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(enrollment => enrollment.RegisteredAt).First());

        return courses
            .Where(course => course.IsPublished)
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
        var now = DateTime.UtcNow;
        var joinedCourses = await GetJoinedCoursesAsync(athleteUserId);

        return joinedCourses
            .SelectMany(course => course.TrainingPlanPublications)
            .Where(publication => publication.VisibleFrom is null || publication.VisibleFrom <= now)
            .Select(publication => publication.TrainingPlanId)
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
            throw new InvalidOperationException(Text("CreateCourseTrainerRequired", "Ein Kurs braucht eine angemeldete Trainer:in."));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException(Text("CreateCourseNameRequired", "Ein Kurs braucht einen Namen."));

        if (request.EndDate.Date < request.StartDate.Date)
            throw new InvalidOperationException(Text("CreateCourseEndBeforeStart", "Das Kursende darf nicht vor dem Kursstart liegen."));

        var courseId = CreateCourseId(request.Name);
        var now = DateTime.UtcNow;
        var course = new CourseDocument
        {
            Id = courseId,
            CourseId = courseId,
            Slug = courseId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Level = request.Level?.Trim() ?? string.Empty,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            CreatedByTrainerUserId = creatorTrainerUserId,
            CreatedAt = now,
            IsPublished = request.IsPublished,
            Trainers =
            {
                new CourseTrainerDocument
                {
                    TrainerUserId = creatorTrainerUserId,
                    DisplayName = NormalizeDisplayName(creatorDisplayName, creatorTrainerUserId),
                    Role = Text("RoleCourseLead", "Kursleitung"),
                    CanManagePlans = true,
                    CanManageEvents = true,
                    CanManageMembers = true
                }
            }
        };

        await _repository.UpsertCourseAsync(course);
        return course;
    }

    public async Task DeleteCourseAsync(string courseId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        if (course.CreatedByTrainerUserId != requestingTrainerUserId)
            throw new InvalidOperationException(Text("DeleteCourseCreatorOnly", "Nur die Trainer:in, die den Kurs erstellt hat, kann ihn loeschen."));

        await _repository.DeleteCourseAsync(course.Id);
    }

    public async Task<CourseEnrollmentDocument> JoinCourseAsync(string courseId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("JoinCourseAthleteRequired", "Ein Kursbeitritt erfordert eine angemeldete Athlet:in."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        var existing = await _repository.GetEnrollmentAsync(course.Id, athleteUserId);
        var enrollment = existing ?? new CourseEnrollmentDocument
        {
            CourseId = course.Id,
            AthleteUserId = athleteUserId,
            RegisteredAt = DateTime.UtcNow
        };

        enrollment.Status = CourseEnrollmentStatus.Active;
        enrollment.CancelledAt = null;

        if (enrollment.RegisteredAt == default)
            enrollment.RegisteredAt = DateTime.UtcNow;

        await _repository.UpsertEnrollmentAsync(enrollment);
        return enrollment;
    }

    public async Task<CourseEnrollmentDocument> LeaveCourseAsync(string courseId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("LeaveCourseAthleteRequired", "Ein Kursaustritt erfordert eine angemeldete Athlet:in."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        var enrollment = await _repository.GetEnrollmentAsync(course.Id, athleteUserId)
            ?? throw new InvalidOperationException(Text("LeaveCourseNotJoined", "Du bist diesem Kurs nicht beigetreten."));

        var cancelledAt = DateTime.UtcNow;
        enrollment.Status = CourseEnrollmentStatus.Cancelled;
        enrollment.CancelledAt = cancelledAt;

        await _repository.UpsertEnrollmentAsync(enrollment);
        await CancelActiveEventRegistrationsAsync(course.Id, athleteUserId, cancelledAt);

        return enrollment;
    }

    public async Task AssignTrainerAsync(string courseId, string trainerUserId, string displayName)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

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
            throw new InvalidOperationException(Text("AddTrainerRequired", "Bitte eine Trainer:in auswaehlen."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequireTrainerManagement(course, requestingTrainerUserId);

        course.Trainers.RemoveAll(trainer => trainer.TrainerUserId == trainerUserId);
        course.Trainers.Add(new CourseTrainerDocument
        {
            TrainerUserId = trainerUserId,
            DisplayName = NormalizeDisplayName(displayName, trainerUserId),
            Role = Text("RoleTrainer", "Trainer:in"),
            CanManagePlans = true,
            CanManageEvents = true,
            CanManageMembers = true
        });

        await _repository.UpsertCourseAsync(course);
    }

    public async Task RemoveTrainerAsync(string courseId, string trainerUserId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        if (course.CreatedByTrainerUserId == trainerUserId)
            throw new InvalidOperationException(Text("RemoveTrainerLeadCannotLeave", "Die Kursleitung kann den Kurs loeschen, aber nicht austreten."));

        if (trainerUserId != requestingTrainerUserId)
            RequireTrainerManagement(course, requestingTrainerUserId);

        var removed = course.Trainers.RemoveAll(trainer => trainer.TrainerUserId == trainerUserId);
        if (removed == 0)
            throw new InvalidOperationException(Text("RemoveTrainerNotAssigned", "Diese Trainer:in ist dem Kurs nicht zugeordnet."));

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
            throw new InvalidOperationException(Text("DateTitleRequired", "Ein Termin braucht einen Titel."));

        if (endsAt <= startsAt)
            throw new InvalidOperationException(Text("DateEndAfterStart", "Das Termin-Ende muss nach dem Start liegen."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

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
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequireEventManagement(course, requestingTrainerUserId);

        var removed = course.Dates.RemoveAll(date => date.Id == dateId);
        if (removed == 0)
            throw new InvalidOperationException(Text("DateNotFound", "Der Termin wurde nicht gefunden."));

        await _repository.UpsertCourseAsync(course);
    }

    public async Task PublishTrainingPlanAsync(string courseId, string trainingPlanId, string publishedByUserId, DateTime? visibleFrom = null)
    {
        if (string.IsNullOrWhiteSpace(trainingPlanId))
            throw new InvalidOperationException(Text("TrainingPlanRequired", "Bitte einen Trainingsplan auswaehlen."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequirePlanManagement(course, publishedByUserId);

        var existing = course.TrainingPlanPublications
            .FirstOrDefault(publication => publication.TrainingPlanId == trainingPlanId);

        if (existing is null)
        {
            course.TrainingPlanPublications.Add(new CourseTrainingPlanPublicationDocument
            {
                TrainingPlanId = trainingPlanId,
                PublishedAt = DateTime.UtcNow,
                PublishedByUserId = publishedByUserId,
                VisibleFrom = visibleFrom
            });
        }
        else
        {
            existing.VisibleFrom = visibleFrom;
            existing.PublishedByUserId = string.IsNullOrWhiteSpace(existing.PublishedByUserId)
                ? publishedByUserId
                : existing.PublishedByUserId;
        }

        await _repository.UpsertCourseAsync(course);
    }

    public async Task RemoveTrainingPlanPublicationAsync(string courseId, string trainingPlanId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequirePlanManagement(course, requestingTrainerUserId);

        var removed = course.TrainingPlanPublications.RemoveAll(publication => publication.TrainingPlanId == trainingPlanId);
        if (removed == 0)
            throw new InvalidOperationException(Text("TrainingPlanNotFound", "Der Trainingsplan wurde nicht gefunden."));

        await _repository.UpsertCourseAsync(course);
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
        DateTime? registrationDeadline = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException(Text("EventTitleRequired", "Ein Event braucht einen Titel."));

        if (endsAt <= startsAt)
            throw new InvalidOperationException(Text("EventEndAfterStart", "Das Event-Ende muss nach dem Start liegen."));

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequireEventManagement(course, createdByUserId);

        var courseEvent = new CourseEventDocument
        {
            CourseId = course.Id,
            Title = title,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Description = description,
            Location = location,
            Capacity = capacity,
            RegistrationDeadline = registrationDeadline,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.UpsertEventAsync(courseEvent);
        return courseEvent;
    }

    public async Task DeleteEventAsync(string courseId, string eventId, string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequireEventManagement(course, requestingTrainerUserId);

        var courseEvent = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "Das Event wurde nicht gefunden."));

        await _repository.DeleteEventAsync(course.Id, courseEvent.Id);
    }

    public async Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsForTrainerAsync(
        string courseId,
        string eventId,
        string requestingTrainerUserId)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException(Text("CourseNotFound", "Der Kurs wurde nicht gefunden."));

        RequireEventManagement(course, requestingTrainerUserId);

        _ = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "Das Event wurde nicht gefunden."));

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
            ?? throw new InvalidOperationException(Text("EventNotFound", "Das Event wurde nicht gefunden."));

        return await _repository.GetEventRegistrationAsync(courseId, eventId, athleteUserId);
    }

    public async Task<CourseEventRegistrationDocument> RegisterForEventAsync(string courseId, string eventId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("RegisterEventAthleteRequired", "Eine Event-Anmeldung erfordert eine angemeldete Athlet:in."));

        var enrollment = await _repository.GetEnrollmentAsync(courseId, athleteUserId);
        if (enrollment?.Status != CourseEnrollmentStatus.Active)
            throw new InvalidOperationException(Text("RegisterEventRequiresEnrollment", "Du musst dem Kurs beigetreten sein, bevor du dich fuer Events anmeldest."));

        var courseEvent = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "Das Event wurde nicht gefunden."));

        if (courseEvent.RegistrationDeadline is not null && courseEvent.RegistrationDeadline < DateTime.UtcNow)
            throw new InvalidOperationException(Text("RegisterEventDeadlinePassed", "Die Anmeldefrist fuer dieses Event ist abgelaufen."));

        var registrations = await _repository.GetEventRegistrationsAsync(courseId, eventId);
        var activeRegistrations = registrations.Count(registration =>
            registration.Status == CourseEventRegistrationStatus.Registered);

        if (courseEvent.Capacity is not null && activeRegistrations >= courseEvent.Capacity)
        {
            var existing = registrations.FirstOrDefault(registration => registration.AthleteUserId == athleteUserId);
            if (existing?.Status != CourseEventRegistrationStatus.Registered)
                throw new InvalidOperationException(Text("RegisterEventFull", "Fuer dieses Event sind keine freien Plaetze mehr verfuegbar."));
        }

        var registration = await _repository.GetEventRegistrationAsync(courseId, eventId, athleteUserId)
            ?? new CourseEventRegistrationDocument
            {
                CourseId = courseId,
                EventId = eventId,
                AthleteUserId = athleteUserId,
                RegisteredAt = DateTime.UtcNow
            };

        registration.Status = CourseEventRegistrationStatus.Registered;
        registration.CancelledAt = null;

        if (registration.RegisteredAt == default)
            registration.RegisteredAt = DateTime.UtcNow;

        await _repository.UpsertEventRegistrationAsync(registration);
        return registration;
    }

    public async Task<CourseEventRegistrationDocument> CancelEventRegistrationAsync(string courseId, string eventId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException(Text("CancelEventAthleteRequired", "Eine Event-Abmeldung erfordert eine angemeldete Athlet:in."));

        _ = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException(Text("EventNotFound", "Das Event wurde nicht gefunden."));

        var registration = await _repository.GetEventRegistrationAsync(courseId, eventId, athleteUserId)
            ?? throw new InvalidOperationException(Text("CancelEventNotRegistered", "Du bist fuer dieses Event nicht angemeldet."));

        if (registration.Status != CourseEventRegistrationStatus.Registered)
            throw new InvalidOperationException(Text("CancelEventNotRegistered", "Du bist fuer dieses Event nicht angemeldet."));

        registration.Status = CourseEventRegistrationStatus.Cancelled;
        registration.CancelledAt = DateTime.UtcNow;

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

    private void RequireTrainerManagement(CourseDocument course, string requestingTrainerUserId)
    {
        var trainer = course.Trainers.FirstOrDefault(trainer => trainer.TrainerUserId == requestingTrainerUserId);
        if (trainer is null || !trainer.CanManageMembers)
            throw new InvalidOperationException(Text("PermissionManageTrainers", "Du darfst die Trainer:innen dieses Kurses nicht verwalten."));
    }

    private void RequireEventManagement(CourseDocument course, string requestingTrainerUserId)
    {
        var trainer = course.Trainers.FirstOrDefault(trainer => trainer.TrainerUserId == requestingTrainerUserId);
        if (trainer is null || !trainer.CanManageEvents)
            throw new InvalidOperationException(Text("PermissionManageEvents", "Du darfst Termine und Events dieses Kurses nicht verwalten."));
    }

    private void RequirePlanManagement(CourseDocument course, string requestingTrainerUserId)
    {
        var trainer = course.Trainers.FirstOrDefault(trainer => trainer.TrainerUserId == requestingTrainerUserId);
        if (trainer is null || !trainer.CanManagePlans)
            throw new InvalidOperationException(Text("PermissionManagePlans", "Du darfst Trainingsplaene dieses Kurses nicht verwalten."));
    }

    private string Text(string key, string fallback)
    {
        if (_localizer is null)
            return fallback;

        var localized = _localizer[key];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }
}
