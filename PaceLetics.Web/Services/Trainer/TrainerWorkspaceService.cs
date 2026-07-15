using AthleteDataAccessLibrary.Contracts;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Enums;
using PaceLetics.RunningAnalysisModule.CodeBase.RunningAnalysis.Interfaces;
using PaceLetics.Web.Services.Achievements;
using PaceLetics.Web.Services.Courses;
using PaceLetics.Web.Services.TrainingFeedback;

namespace PaceLetics.Web.Services.Trainer;

public sealed record TrainerScheduleItem(string Title, string CourseName, DateTime StartsAt, string Location, string Href);
public sealed record TrainerDeviation(string AthleteName, int MissedSessions, int PlannedSessions, string Href);
public sealed record TrainerFeedbackItem(string AthleteName, string TrainingName, int Effort, TrainingFeeling Feeling, string Comment, DateTime CompletedAt, string Href);
public sealed record TrainerSearchItem(string Kind, string Title, string Subtitle, string Href);

public sealed record TrainerWorkspaceSnapshot(
    IReadOnlyList<TrainerScheduleItem> Today,
    int NewMessageCount,
    int PendingMembershipCount,
    int PendingAnalysisCount,
    IReadOnlyList<TrainerDeviation> Deviations,
    IReadOnlyList<TrainerFeedbackItem> RecentFeedback,
    IReadOnlyList<TrainerSearchItem> SearchItems)
{
    public static TrainerWorkspaceSnapshot Empty { get; } = new([], 0, 0, 0, [], [], []);
}

public sealed class TrainerWorkspaceService
{
    private readonly ICourseService _courses;
    private readonly ICourseRepository _courseRepository;
    private readonly IGroupService _groups;
    private readonly ITrainingPlanService _plans;
    private readonly IAchievementService _achievements;
    private readonly ITrainingFeedbackService _feedback;
    private readonly IAthleteData _athletes;
    private readonly IRunningAnalysisRepository _analysisRepository;
    private readonly IRunningAnalysisService _analysisService;

    public TrainerWorkspaceService(
        ICourseService courses,
        ICourseRepository courseRepository,
        IGroupService groups,
        ITrainingPlanService plans,
        IAchievementService achievements,
        ITrainingFeedbackService feedback,
        IAthleteData athletes,
        IRunningAnalysisRepository analysisRepository,
        IRunningAnalysisService analysisService)
    {
        _courses = courses;
        _courseRepository = courseRepository;
        _groups = groups;
        _plans = plans;
        _achievements = achievements;
        _feedback = feedback;
        _athletes = athletes;
        _analysisRepository = analysisRepository;
        _analysisService = analysisService;
    }

    public async Task<TrainerWorkspaceSnapshot> LoadAsync(string trainerUserId)
    {
        if (string.IsNullOrWhiteSpace(trainerUserId))
            return TrainerWorkspaceSnapshot.Empty;

        var courses = await _courses.GetCoursesForTrainerAsync(trainerUserId);
        var groups = await _groups.GetGroupsForTrainerAsync(trainerUserId);
        var courseDataTasks = courses.Select(async course => new
        {
            Course = course,
            Enrollments = await _courseRepository.GetEnrollmentsForCourseAsync(course.Id),
            Events = await _courses.GetEventsAsync(course.Id)
        }).ToList();
        var groupDataTasks = groups.Select(async group => new
        {
            Group = group,
            Memberships = await _groups.GetMembershipsForGroupAsync(group.Id, trainerUserId)
        }).ToList();

        var courseData = await Task.WhenAll(courseDataTasks);
        var groupData = await Task.WhenAll(groupDataTasks);
        var activeAthleteIds = courseData
            .SelectMany(item => item.Enrollments)
            .Where(item => item.Status == CourseEnrollmentStatus.Active)
            .Select(item => item.AthleteUserId)
            .Concat(groupData.SelectMany(item => item.Memberships)
                .Where(item => item.Status == GroupMembershipStatus.Active)
                .Select(item => item.AthleteUserId))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var athleteModels = await _athletes.GetAthletes();
        var athleteById = new Dictionary<string, AthleteModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var athlete in athleteModels)
        {
            if (!string.IsNullOrWhiteSpace(athlete.Id)) athleteById[athlete.Id] = athlete;
            if (!string.IsNullOrWhiteSpace(athlete.AthleteId)) athleteById[athlete.AthleteId] = athlete;
        }

        var athleteInsightTasks = activeAthleteIds.Select(id => LoadAthleteInsightAsync(id, GetAthleteName(id, athleteById))).ToList();
        var athleteInsights = await Task.WhenAll(athleteInsightTasks);
        var feedbackItems = athleteInsights
            .SelectMany(item => item.Feedback)
            .OrderByDescending(item => item.CompletedAt)
            .Take(8)
            .ToList();

        var today = DateTime.Today;
        var schedule = courseData
            .SelectMany(item => item.Course.Dates.Select(date => new TrainerScheduleItem(
                date.Title, item.Course.Name, date.StartsAt, date.Location, $"/Trainers/courses?course={Uri.EscapeDataString(item.Course.Id)}")))
            .Concat(courseData.SelectMany(item => item.Events.Select(courseEvent => new TrainerScheduleItem(
                courseEvent.Title, item.Course.Name, courseEvent.StartsAt, courseEvent.Location, $"/Trainers/courses?course={Uri.EscapeDataString(item.Course.Id)}"))))
            .Where(item => item.StartsAt.Date == today)
            .OrderBy(item => item.StartsAt)
            .ToList();

        var analysisEvents = courseData
            .SelectMany(item => item.Events.Select(courseEvent => new AnalysisEventItem(item.Course, courseEvent)))
            .Where(item => string.Equals(item.Event.EventType, CourseEventTypes.RunningAnalysis, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var pendingAnalysisCount = await CountPendingAnalysesAsync(analysisEvents.Select(item => item.Event));
        var pendingMemberships = groupData.SelectMany(item => item.Memberships)
            .Count(item => item.Status == GroupMembershipStatus.Pending);
        var allFeedback = athleteInsights.SelectMany(item => item.Feedback).ToList();
        var newMessageCount = allFeedback.Count(item =>
            !string.IsNullOrWhiteSpace(item.Comment) && item.CompletedAt >= DateTime.UtcNow.AddDays(-7));

        var searchItems = BuildSearchItems(courses, groups, athleteById, activeAthleteIds, analysisEvents);
        return new TrainerWorkspaceSnapshot(
            schedule,
            newMessageCount,
            pendingMemberships,
            pendingAnalysisCount,
            athleteInsights.Where(item => item.Deviation is not null).Select(item => item.Deviation!).OrderByDescending(item => item.MissedSessions).ToList(),
            feedbackItems,
            searchItems);
    }

    private async Task<AthleteInsight> LoadAthleteInsightAsync(string athleteUserId, string athleteName)
    {
        var plansTask = _plans.LoadTrainingPlansForUserAsync(athleteUserId);
        var completionsTask = _achievements.GetTrainingSessionCompletionsForAthleteAsync(athleteUserId);
        var feedbackTask = _feedback.GetRecentAsync(athleteUserId, 28);
        await Task.WhenAll(plansTask, completionsTask, feedbackTask);

        var plans = await plansTask;
        var completions = await completionsTask;
        var completedKeys = completions.Select(item => $"{item.PlanId}:{item.SessionId}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var from = DateTime.Today.AddDays(-14);
        var recentPlanned = plans
            .SelectMany(plan => plan.Sessions.Select(session => new { PlanId = plan.Id, Session = session }))
            .Where(item => (item.Session.Appointment.StartsAt ?? item.Session.Date).Date >= from)
            .Where(item => (item.Session.Appointment.StartsAt ?? item.Session.Date).Date < DateTime.Today)
            .ToList();
        var missed = recentPlanned.Count(item => !completedKeys.Contains($"{item.PlanId}:{item.Session.Id}"));
        var deviation = missed == 0 ? null : new TrainerDeviation(
            athleteName,
            missed,
            recentPlanned.Count,
            $"/Trainers/training-plans?athlete={Uri.EscapeDataString(athleteUserId)}");
        var feedback = (await feedbackTask).Select(item => new TrainerFeedbackItem(
            athleteName,
            item.TrainingName,
            item.Effort,
            item.Feeling,
            item.Comment,
            item.CompletedAt,
            $"/Trainers/training-plans?athlete={Uri.EscapeDataString(athleteUserId)}")).ToList();
        return new AthleteInsight(deviation, feedback);
    }

    private async Task<int> CountPendingAnalysesAsync(IEnumerable<CourseEventDocument> events)
    {
        var pending = 0;
        foreach (var courseEvent in events.Where(item => item.StartsAt <= DateTime.Now))
        {
            var capture = await _analysisRepository.GetCaptureSessionByExternalEventIdAsync(courseEvent.Id);
            if (capture is null)
            {
                pending++;
                continue;
            }

            var roster = await _analysisService.GetRosterAsync(capture.Id);
            pending += roster.Count(item =>
                (item.RecordingCount > 0 || !string.IsNullOrWhiteSpace(item.SideRecordingUrl) || !string.IsNullOrWhiteSpace(item.RearRecordingUrl))
                && item.ResultStatus != RunningAnalysisResultStatus.Completed);
        }
        return pending;
    }

    private IReadOnlyList<TrainerSearchItem> BuildSearchItems(
        IReadOnlyList<CourseDocument> courses,
        IReadOnlyList<GroupDocument> groups,
        IReadOnlyDictionary<string, AthleteModel> athleteById,
        IReadOnlyList<string> athleteIds,
        IEnumerable<AnalysisEventItem> analysisEvents)
    {
        var items = new List<TrainerSearchItem>();
        items.AddRange(courses.Select(course => new TrainerSearchItem("course", course.Name, course.Description, $"/Trainers/courses?course={Uri.EscapeDataString(course.Id)}")));
        items.AddRange(groups.Select(group => new TrainerSearchItem("group", group.Name, group.Description, "/Trainers/groups")));
        items.AddRange(_plans.LoadTrainingPlanDefinitions().Select(plan => new TrainerSearchItem("plan", plan.Name, string.Empty, "/Trainers/training-plans")));
        items.AddRange(athleteIds.Select(id => new TrainerSearchItem("athlete", GetAthleteName(id, athleteById), id, $"/Trainers/training-plans?athlete={Uri.EscapeDataString(id)}")));
        foreach (var item in analysisEvents)
            items.Add(new TrainerSearchItem("analysis", item.Event.Title, item.Course.Name, $"/Trainers/courses/{item.Course.Id}/events/{item.Event.Id}/running-analysis/analysis"));
        return items.OrderBy(item => item.Title).ToList();
    }

    private static string GetAthleteName(string id, IReadOnlyDictionary<string, AthleteModel> athleteById) =>
        athleteById.TryGetValue(id, out var athlete) && !string.IsNullOrWhiteSpace(athlete.Name)
            ? athlete.Name
            : id;

    private sealed record AthleteInsight(TrainerDeviation? Deviation, IReadOnlyList<TrainerFeedbackItem> Feedback);
    private sealed record AnalysisEventItem(CourseDocument Course, CourseEventDocument Event);
}
