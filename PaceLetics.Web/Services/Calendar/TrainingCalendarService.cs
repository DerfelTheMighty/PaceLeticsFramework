using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Web.Services.Calendar;

public sealed class TrainingCalendarService : ITrainingCalendarService
{
    private readonly ICourseService _courseService;
    private readonly ITrainingPlanService _trainingPlanService;

    public TrainingCalendarService(
        ICourseService courseService,
        ITrainingPlanService trainingPlanService)
    {
        _courseService = courseService;
        _trainingPlanService = trainingPlanService;
    }

    public async Task<IReadOnlyList<TrainingCalendarItem>> GetCalendarItemsForAthleteAsync(string athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return Array.Empty<TrainingCalendarItem>();

        var now = DateTime.UtcNow;
        var joinedCourses = await _courseService.GetJoinedCoursesAsync(athleteUserId);
        var plans = await _trainingPlanService.LoadTrainingPlansForUserAsync(athleteUserId);
        var courseNamesByPlanId = BuildCourseNamesByPlanId(joinedCourses, now);
        var items = new List<TrainingCalendarItem>();

        foreach (var plan in plans)
        {
            courseNamesByPlanId.TryGetValue(plan.Id, out var courseNames);
            var courseName = courseNames is null ? string.Empty : string.Join(", ", courseNames);

            foreach (var session in plan.Sessions)
            {
                items.Add(new TrainingCalendarItem
                {
                    Id = $"training:{plan.Id}:{session.Id}",
                    Kind = TrainingCalendarItemKinds.TrainingSession,
                    Title = session.Name,
                    Description = BuildSessionDescription(session),
                    StartsAt = session.Appointment.StartsAt ?? session.Date.Date,
                    EndsAt = session.Appointment.EndsAt,
                    HasTime = session.Appointment.StartsAt is not null,
                    Location = session.Appointment.Location,
                    CourseName = courseName,
                    PlanId = plan.Id,
                    PlanName = plan.Name,
                    SessionId = session.Id
                });
            }
        }

        foreach (var course in joinedCourses)
        {
            foreach (var date in course.Dates)
            {
                items.Add(new TrainingCalendarItem
                {
                    Id = $"course-date:{course.Id}:{date.Id}",
                    Kind = TrainingCalendarItemKinds.CourseDate,
                    Title = date.Title,
                    Description = date.Notes,
                    StartsAt = date.StartsAt,
                    EndsAt = date.EndsAt,
                    HasTime = true,
                    Location = date.Location,
                    CourseId = course.Id,
                    CourseName = course.Name
                });
            }

            var courseEvents = await _courseService.GetEventsAsync(course.Id);
            foreach (var courseEvent in courseEvents)
            {
                var registration = await _courseService.GetEventRegistrationForAthleteAsync(
                    course.Id,
                    courseEvent.Id,
                    athleteUserId);

                items.Add(new TrainingCalendarItem
                {
                    Id = $"course-event:{course.Id}:{courseEvent.Id}",
                    Kind = TrainingCalendarItemKinds.CourseEvent,
                    Title = courseEvent.Title,
                    Description = courseEvent.Description,
                    StartsAt = courseEvent.StartsAt,
                    EndsAt = courseEvent.EndsAt,
                    HasTime = true,
                    Location = courseEvent.Location,
                    CourseId = course.Id,
                    CourseName = course.Name,
                    EventId = courseEvent.Id,
                    EventType = courseEvent.EventType,
                    IsRegistered = registration?.Status == CourseEventRegistrationStatus.Registered
                });
            }
        }

        return items
            .OrderBy(item => item.StartsAt)
            .ThenBy(item => item.Kind)
            .ThenBy(item => item.Title)
            .ToList();
    }

    private static Dictionary<string, IReadOnlyList<string>> BuildCourseNamesByPlanId(
        IReadOnlyList<CourseDocument> courses,
        DateTime now)
    {
        return courses
            .SelectMany(course => course.TrainingPlanPublications
                .Where(publication => publication.IsVisibleInCourse(course.Id, now))
                .Select(publication => new
                {
                    publication.TrainingPlanId,
                    course.Name
                }))
            .Where(item => !string.IsNullOrWhiteSpace(item.TrainingPlanId))
            .GroupBy(item => item.TrainingPlanId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(item => item.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildSessionDescription(TrainingSession session)
    {
        var parts = new List<string>();

        if (session.PrimaryRun is not null)
            parts.Add($"{session.PrimaryRun.TotalDistance / 1000.0:0.0} km");

        if (session.Workouts.Count > 0)
            parts.Add($"{session.Workouts.Count} Workout(s)");

        if (session.Warmup.Count > 0)
            parts.Add($"{session.Warmup.Count} Warm-up");

        if (session.Drills.Count > 0)
            parts.Add($"{session.Drills.Count} Drill(s)");

        return string.Join(" - ", parts);
    }
}
