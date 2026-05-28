namespace PaceLetics.Web.Services.Courses;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;

    public CourseService(ICourseRepository repository)
    {
        _repository = repository;
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

    public async Task<CourseEnrollmentDocument> JoinCourseAsync(string courseId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException("Ein Kursbeitritt erfordert eine angemeldete Athlet:in.");

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException("Der Kurs wurde nicht gefunden.");

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

    public async Task AssignTrainerAsync(string courseId, string trainerUserId, string displayName)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException("Der Kurs wurde nicht gefunden.");

        course.Trainers.RemoveAll(trainer => trainer.TrainerUserId == trainerUserId);
        course.Trainers.Add(new CourseTrainerDocument
        {
            TrainerUserId = trainerUserId,
            DisplayName = displayName
        });

        await _repository.UpsertCourseAsync(course);
    }

    public async Task PublishTrainingPlanAsync(string courseId, string trainingPlanId, string publishedByUserId, DateTime? visibleFrom = null)
    {
        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException("Der Kurs wurde nicht gefunden.");

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
            throw new InvalidOperationException("Ein Event braucht einen Titel.");

        if (endsAt <= startsAt)
            throw new InvalidOperationException("Das Event-Ende muss nach dem Start liegen.");

        var course = await _repository.GetCourseAsync(courseId)
            ?? throw new InvalidOperationException("Der Kurs wurde nicht gefunden.");

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

    public async Task<CourseEventRegistrationDocument> RegisterForEventAsync(string courseId, string eventId, string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException("Eine Event-Anmeldung erfordert eine angemeldete Athlet:in.");

        var enrollment = await _repository.GetEnrollmentAsync(courseId, athleteUserId);
        if (enrollment?.Status != CourseEnrollmentStatus.Active)
            throw new InvalidOperationException("Du musst dem Kurs beigetreten sein, bevor du dich für Events anmeldest.");

        var courseEvent = await _repository.GetEventAsync(courseId, eventId)
            ?? throw new InvalidOperationException("Das Event wurde nicht gefunden.");

        if (courseEvent.RegistrationDeadline is not null && courseEvent.RegistrationDeadline < DateTime.UtcNow)
            throw new InvalidOperationException("Die Anmeldefrist für dieses Event ist abgelaufen.");

        var registrations = await _repository.GetEventRegistrationsAsync(courseId, eventId);
        var activeRegistrations = registrations.Count(registration =>
            registration.Status == CourseEventRegistrationStatus.Registered);

        if (courseEvent.Capacity is not null && activeRegistrations >= courseEvent.Capacity)
        {
            var existing = registrations.FirstOrDefault(registration => registration.AthleteUserId == athleteUserId);
            if (existing?.Status != CourseEventRegistrationStatus.Registered)
                throw new InvalidOperationException("Für dieses Event sind keine freien Plätze mehr verfügbar.");
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
}
