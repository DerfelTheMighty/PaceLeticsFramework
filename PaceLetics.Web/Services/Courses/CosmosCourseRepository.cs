using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;
using PaceLetics.Web.Services.Mates;

namespace PaceLetics.Web.Services.Courses;

public sealed class CosmosCourseRepository : ICourseRepository, IMateRepository
{
    private readonly IDataAccess _db;
    private readonly AthleteDataOptions _options;

    public CosmosCourseRepository(IDataAccess db, AthleteDataOptions options)
    {
        _db = db;
        _options = options;
        _options.Validate();
    }

    public async Task<IReadOnlyList<CourseDocument>> GetCoursesAsync()
    {
        await EnsureDefaultCoursesAsync();
        var courses = await _db.LoadData<CourseDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            CourseDocumentTypes.Course);

        return courses
            .Where(course => !string.IsNullOrWhiteSpace(course.Id))
            .OrderBy(course => course.StartDate)
            .ThenBy(course => course.Name)
            .ToList();
    }

    public async Task<CourseDocument?> GetCourseAsync(string courseId)
    {
        await EnsureDefaultCoursesAsync();
        return await _db.LoadItem<CourseDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            courseId,
            courseId);
    }

    public Task UpsertCourseAsync(CourseDocument course)
    {
        NormalizeCourse(course);
        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            course,
            course.CourseId);
    }

    public async Task DeleteCourseAsync(string courseId)
    {
        var registrations = await LoadPartitionItems<CourseEventRegistrationDocument>(
            courseId,
            CourseDocumentTypes.EventRegistration);
        var mateAvailabilities = await LoadPartitionItems<MateAvailabilityDocument>(
            courseId,
            CourseDocumentTypes.MateAvailability);
        var events = await LoadPartitionItems<CourseEventDocument>(courseId, CourseDocumentTypes.Event);
        var enrollments = await LoadPartitionItems<CourseEnrollmentDocument>(
            courseId,
            CourseDocumentTypes.Enrollment);

        foreach (var availability in mateAvailabilities)
            await _db.DeleteItem<MateAvailabilityDocument>(
                _options.DatabaseName,
                _options.CourseContainerName,
                availability.Id,
                courseId);

        foreach (var registration in registrations)
            await _db.DeleteItem<CourseEventRegistrationDocument>(
                _options.DatabaseName,
                _options.CourseContainerName,
                registration.Id,
                courseId);

        foreach (var courseEvent in events)
            await _db.DeleteItem<CourseEventDocument>(
                _options.DatabaseName,
                _options.CourseContainerName,
                courseEvent.Id,
                courseId);

        foreach (var enrollment in enrollments)
            await _db.DeleteItem<CourseEnrollmentDocument>(
                _options.DatabaseName,
                _options.CourseContainerName,
                enrollment.Id,
                courseId);

        await _db.DeleteItem<CourseDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            courseId,
            courseId);
    }

    public async Task<IReadOnlyList<CourseEnrollmentDocument>> GetEnrollmentsForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<CourseEnrollmentDocument>();

        var enrollments = await _db.LoadData<CourseEnrollmentDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            CourseDocumentTypes.Enrollment);

        return enrollments
            .Where(enrollment => enrollment.AthleteUserId == athleteUserId)
            .ToList();
    }

    public async Task<IReadOnlyList<CourseEnrollmentDocument>> GetEnrollmentsForCourseAsync(string courseId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            return Array.Empty<CourseEnrollmentDocument>();

        var enrollments = await _db.LoadPartitionData<CourseEnrollmentDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            courseId,
            CourseDocumentTypes.Enrollment);

        return enrollments
            .Where(enrollment => enrollment.CourseId == courseId)
            .ToList();
    }

    public Task<CourseEnrollmentDocument?> GetEnrollmentAsync(string courseId, string athleteUserId)
    {
        return _db.LoadItem<CourseEnrollmentDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            CourseDocumentIds.Enrollment(courseId, athleteUserId),
            courseId);
    }

    public Task UpsertEnrollmentAsync(CourseEnrollmentDocument enrollment)
    {
        enrollment.DocumentType = CourseDocumentTypes.Enrollment;
        enrollment.Id = CourseDocumentIds.Enrollment(enrollment.CourseId, enrollment.AthleteUserId);

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            enrollment,
            enrollment.CourseId);
    }

    public Task<IReadOnlyList<CourseEventDocument>> GetEventsAsync(string courseId)
    {
        return LoadPartitionItems<CourseEventDocument>(courseId, CourseDocumentTypes.Event);
    }

    public Task<CourseEventDocument?> GetEventAsync(string courseId, string eventId)
    {
        return _db.LoadItem<CourseEventDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            eventId,
            courseId);
    }

    public Task UpsertEventAsync(CourseEventDocument courseEvent)
    {
        courseEvent.DocumentType = CourseDocumentTypes.Event;
        if (string.IsNullOrWhiteSpace(courseEvent.Id))
            courseEvent.Id = Guid.NewGuid().ToString("N");

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            courseEvent,
            courseEvent.CourseId);
    }

    public async Task DeleteEventAsync(string courseId, string eventId)
    {
        var registrations = await GetEventRegistrationsAsync(courseId, eventId);

        foreach (var registration in registrations)
        {
            await _db.DeleteItem<CourseEventRegistrationDocument>(
                _options.DatabaseName,
                _options.CourseContainerName,
                registration.Id,
                courseId);
        }

        await _db.DeleteItem<CourseEventDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            eventId,
            courseId);
    }

    public async Task<IReadOnlyList<CourseEventRegistrationDocument>> GetEventRegistrationsAsync(string courseId, string eventId)
    {
        var registrations = await LoadPartitionItems<CourseEventRegistrationDocument>(
            courseId,
            CourseDocumentTypes.EventRegistration);

        return registrations
            .Where(registration => registration.EventId == eventId)
            .ToList();
    }

    public Task<CourseEventRegistrationDocument?> GetEventRegistrationAsync(string courseId, string eventId, string athleteUserId)
    {
        return _db.LoadItem<CourseEventRegistrationDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            CourseDocumentIds.EventRegistration(eventId, athleteUserId),
            courseId);
    }

    public Task UpsertEventRegistrationAsync(CourseEventRegistrationDocument registration)
    {
        registration.DocumentType = CourseDocumentTypes.EventRegistration;
        registration.Id = CourseDocumentIds.EventRegistration(registration.EventId, registration.AthleteUserId);

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            registration,
            registration.CourseId);
    }

    public Task<IReadOnlyList<MateAvailabilityDocument>> GetMateAvailabilitiesForCourseAsync(string courseId)
    {
        return LoadPartitionItems<MateAvailabilityDocument>(courseId, CourseDocumentTypes.MateAvailability);
    }

    public async Task<IReadOnlyList<MateAvailabilityDocument>> GetMateAvailabilitiesForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<MateAvailabilityDocument>();

        var availabilities = await _db.LoadData<MateAvailabilityDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            CourseDocumentTypes.MateAvailability);

        return availabilities
            .Where(availability => availability.AthleteUserId == athleteUserId)
            .ToList();
    }

    public Task<MateAvailabilityDocument?> GetMateAvailabilityAsync(string courseId, string availabilityId)
    {
        return _db.LoadItem<MateAvailabilityDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            availabilityId,
            courseId);
    }

    public Task UpsertMateAvailabilityAsync(MateAvailabilityDocument availability)
    {
        availability.DocumentType = CourseDocumentTypes.MateAvailability;
        if (string.IsNullOrWhiteSpace(availability.Id))
        {
            availability.Id = MateDocumentIds.Availability(
                availability.CourseId,
                availability.AthleteUserId,
                availability.PlanId,
                availability.SessionId);
        }

        return _db.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            availability,
            availability.CourseId);
    }

    public Task DeleteMateAvailabilityAsync(string courseId, string availabilityId)
    {
        return _db.DeleteItem<MateAvailabilityDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            availabilityId,
            courseId);
    }

    private async Task EnsureDefaultCoursesAsync()
    {
        var existingCourses = await _db.LoadData<CourseDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            CourseDocumentTypes.Course);
        var existingIds = existingCourses.Select(course => course.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var course in CourseSeedData.CreateDefaultCourses())
        {
            if (existingIds.Contains(course.Id))
                continue;

            await UpsertCourseAsync(course);
        }
    }

    private async Task<IReadOnlyList<T>> LoadPartitionItems<T>(string courseId, string documentType)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            return Array.Empty<T>();

        var items = await _db.LoadPartitionData<T>(
            _options.DatabaseName,
            _options.CourseContainerName,
            courseId,
            documentType);

        return items.ToList();
    }

    private static void NormalizeCourse(CourseDocument course)
    {
        if (string.IsNullOrWhiteSpace(course.Id))
            course.Id = course.CourseId;

        if (string.IsNullOrWhiteSpace(course.CourseId))
            course.CourseId = course.Id;

        course.DocumentType = CourseDocumentTypes.Course;
        course.Slug = string.IsNullOrWhiteSpace(course.Slug) ? course.Id : course.Slug;
    }
}

public static class CourseDocumentIds
{
    public static string Enrollment(string courseId, string athleteUserId)
    {
        return $"enrollment:{courseId}:{athleteUserId}";
    }

    public static string EventRegistration(string eventId, string athleteUserId)
    {
        return $"event-registration:{eventId}:{athleteUserId}";
    }
}
