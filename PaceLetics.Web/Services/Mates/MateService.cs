using AthleteDataAccessLibrary.Contracts;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services.Courses;

namespace PaceLetics.Web.Services.Mates;

public sealed class MateService : IMateService
{
    private const int MaxDateDeltaDays = 7;
    private const int MaxPaceDeltaSeconds = 25;
    private const double MaxDistanceDeltaRatio = 0.35;

    private static readonly SegmentType[] MatchSegmentTypes =
    {
        SegmentType.Dauerlauf,
        SegmentType.Intervall
    };

    private readonly ICourseService _courseService;
    private readonly IMateRepository _mateRepository;
    private readonly ITrainingPlanService _trainingPlanService;
    private readonly IAthleteData _athleteData;
    private readonly TimeProvider _timeProvider;

    public MateService(
        ICourseService courseService,
        IMateRepository mateRepository,
        ITrainingPlanService trainingPlanService,
        IAthleteData athleteData,
        TimeProvider? timeProvider = null)
    {
        _courseService = courseService;
        _mateRepository = mateRepository;
        _trainingPlanService = trainingPlanService;
        _athleteData = athleteData;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<MateOverview> GetOverviewAsync(string? athleteUserId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return new MateOverview();

        var userId = athleteUserId.Trim();
        var joinedCourses = await _courseService.GetJoinedCoursesAsync(userId);
        var visiblePlans = await _trainingPlanService.LoadTrainingPlansForUserAsync(userId);
        var ownAvailabilities = await GetActiveOwnAvailabilitiesAsync(userId);
        var shareableSessions = BuildShareableSessions(joinedCourses, visiblePlans, ownAvailabilities);
        var matches = await BuildMatchesAsync(userId, joinedCourses, ownAvailabilities);

        return new MateOverview
        {
            ShareableSessions = shareableSessions,
            MyAvailabilities = ownAvailabilities,
            Matches = matches
        };
    }

    public async Task<MateAvailabilityDocument> ShareSessionAsync(string? athleteUserId, MateShareRequest request)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            throw new InvalidOperationException("Mate sharing requires a signed-in athlete.");

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var userId = athleteUserId.Trim();
        var courseId = request.CourseId?.Trim() ?? string.Empty;
        var planId = request.PlanId?.Trim() ?? string.Empty;
        var sessionId = request.SessionId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(planId) || string.IsNullOrWhiteSpace(sessionId))
            throw new InvalidOperationException("Course, plan, and session are required for Mate sharing.");

        var joinedCourses = await _courseService.GetJoinedCoursesAsync(userId);
        var course = joinedCourses.FirstOrDefault(course => string.Equals(course.Id, courseId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The selected course is not available for this athlete.");

        if (!GetVisiblePlanIds(course).Contains(planId))
            throw new InvalidOperationException("The selected training plan is not visible in this course.");

        var plans = await _trainingPlanService.LoadTrainingPlansForUserAsync(userId);
        var plan = plans.FirstOrDefault(plan => string.Equals(plan.Id, planId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The selected training plan is not available for this athlete.");
        var session = plan.Sessions.FirstOrDefault(session => string.Equals(session.Id, sessionId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The selected training session was not found.");

        if (session.PrimaryRun is null)
            throw new InvalidOperationException("Only running sessions can be shared with Mate.");

        var athlete = await _athleteData.GetAthlete(userId);
        var availabilityId = MateDocumentIds.Availability(course.Id, userId, plan.Id, session.Id);
        var existing = await _mateRepository.GetMateAvailabilityAsync(course.Id, availabilityId);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var availability = CreateAvailability(
            course,
            plan,
            session,
            athlete,
            userId,
            request.Notes,
            existing?.CreatedAt ?? now);

        availability.Id = availabilityId;
        availability.UpdatedAt = now;
        availability.IsActive = true;

        await _mateRepository.UpsertMateAvailabilityAsync(availability);
        return availability;
    }

    public async Task RemoveAvailabilityAsync(string? athleteUserId, string availabilityId)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId) || string.IsNullOrWhiteSpace(availabilityId))
            return;

        var ownAvailabilities = await _mateRepository.GetMateAvailabilitiesForAthleteAsync(athleteUserId.Trim());
        var availability = ownAvailabilities.FirstOrDefault(availability =>
            string.Equals(availability.Id, availabilityId, StringComparison.OrdinalIgnoreCase));

        if (availability is null)
            return;

        await _mateRepository.DeleteMateAvailabilityAsync(availability.CourseId, availability.Id);
    }

    private async Task<IReadOnlyList<MateAvailabilityDocument>> GetActiveOwnAvailabilitiesAsync(string athleteUserId)
    {
        return (await _mateRepository.GetMateAvailabilitiesForAthleteAsync(athleteUserId))
            .Where(IsActiveAndCurrent)
            .OrderBy(availability => GetEffectiveStart(availability))
            .ThenBy(availability => availability.SessionName)
            .ToList();
    }

    private async Task<IReadOnlyList<MateMatch>> BuildMatchesAsync(
        string athleteUserId,
        IReadOnlyList<CourseDocument> joinedCourses,
        IReadOnlyList<MateAvailabilityDocument> ownAvailabilities)
    {
        if (ownAvailabilities.Count == 0 || joinedCourses.Count == 0)
            return Array.Empty<MateMatch>();

        var candidates = new List<MateAvailabilityDocument>();
        foreach (var course in joinedCourses)
        {
            candidates.AddRange(await _mateRepository.GetMateAvailabilitiesForCourseAsync(course.Id));
        }

        return ownAvailabilities
            .SelectMany(own => candidates
                .Where(candidate => IsCandidate(athleteUserId, own, candidate))
                .Select(candidate => CreateMatch(own, candidate)))
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.DayDelta)
            .ThenBy(match => match.PaceDeltaSeconds ?? int.MaxValue)
            .ThenBy(match => match.MateSession.AthleteDisplayName)
            .Take(20)
            .ToList();
    }

    private IReadOnlyList<MateShareableSession> BuildShareableSessions(
        IReadOnlyList<CourseDocument> joinedCourses,
        IReadOnlyList<TrainingPlan> visiblePlans,
        IReadOnlyList<MateAvailabilityDocument> ownAvailabilities)
    {
        var plansById = visiblePlans.ToDictionary(plan => plan.Id, StringComparer.OrdinalIgnoreCase);
        var activeAvailabilityByKey = ownAvailabilities
            .GroupBy(availability => ShareKey(availability.CourseId, availability.PlanId, availability.SessionId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var sessions = new List<MateShareableSession>();

        foreach (var course in joinedCourses)
        {
            foreach (var planId in GetVisiblePlanIds(course))
            {
                if (!plansById.TryGetValue(planId, out var plan))
                    continue;

                foreach (var session in plan.Sessions.Where(session => session.PrimaryRun is not null && session.Date.Date >= LocalToday))
                {
                    var snapshot = BuildRunSnapshot(session.PrimaryRun!, null);
                    var key = ShareKey(course.Id, plan.Id, session.Id);
                    activeAvailabilityByKey.TryGetValue(key, out var availability);

                    sessions.Add(new MateShareableSession
                    {
                        CourseId = course.Id,
                        CourseName = course.Name,
                        PlanId = plan.Id,
                        PlanName = plan.Name,
                        SessionId = session.Id,
                        SessionName = session.Name,
                        SessionDate = session.Date,
                        StartsAt = session.Appointment.StartsAt,
                        Location = session.Appointment.Location,
                        DistanceMeters = snapshot.DistanceMeters,
                        PaceKey = snapshot.PaceKey,
                        PaceSecondsPerKilometer = snapshot.PaceSecondsPerKilometer,
                        IsShared = availability is not null,
                        AvailabilityId = availability?.Id
                    });
                }
            }
        }

        return sessions
            .GroupBy(session => ShareKey(session.CourseId, session.PlanId, session.SessionId), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(session => session.StartsAt ?? session.SessionDate)
            .ThenBy(session => session.CourseName)
            .ThenBy(session => session.SessionName)
            .ToList();
    }

    private static MateAvailabilityDocument CreateAvailability(
        CourseDocument course,
        TrainingPlan plan,
        TrainingSession session,
        AthleteModel? athlete,
        string athleteUserId,
        string? notes,
        DateTime createdAt)
    {
        var snapshot = BuildRunSnapshot(session.PrimaryRun!, athlete?.PaceModel);

        return new MateAvailabilityDocument
        {
            CourseId = course.Id,
            DocumentType = MateDocumentTypes.Availability,
            AthleteUserId = athleteUserId,
            AthleteDisplayName = ResolveDisplayName(athlete, athleteUserId),
            PlanId = plan.Id,
            PlanName = plan.Name,
            SessionId = session.Id,
            SessionName = session.Name,
            CourseName = course.Name,
            SessionDate = session.Date,
            StartsAt = session.Appointment.StartsAt,
            EndsAt = session.Appointment.EndsAt,
            Location = session.Appointment.Location,
            DistanceMeters = snapshot.DistanceMeters,
            PaceKey = snapshot.PaceKey,
            PaceSecondsPerKilometer = snapshot.PaceSecondsPerKilometer,
            Notes = notes?.Trim() ?? string.Empty,
            CreatedAt = createdAt
        };
    }

    private static MateMatch? CreateMatch(MateAvailabilityDocument own, MateAvailabilityDocument candidate)
    {
        var dayDelta = Math.Abs((GetEffectiveStart(candidate).Date - GetEffectiveStart(own).Date).Days);
        if (dayDelta > MaxDateDeltaDays)
            return null;

        var distanceDelta = Math.Abs(candidate.DistanceMeters - own.DistanceMeters);
        var distanceRatio = own.DistanceMeters <= 0 || candidate.DistanceMeters <= 0
            ? 0
            : distanceDelta / (double)Math.Max(own.DistanceMeters, candidate.DistanceMeters);
        if (distanceRatio > MaxDistanceDeltaRatio)
            return null;

        int? paceDelta = null;
        var paceMatches = false;
        if (own.PaceSecondsPerKilometer is not null && candidate.PaceSecondsPerKilometer is not null)
        {
            paceDelta = Math.Abs(own.PaceSecondsPerKilometer.Value - candidate.PaceSecondsPerKilometer.Value);
            paceMatches = paceDelta <= MaxPaceDeltaSeconds;
        }
        else if (!string.IsNullOrWhiteSpace(own.PaceKey) && !string.IsNullOrWhiteSpace(candidate.PaceKey))
        {
            paceMatches = string.Equals(own.PaceKey, candidate.PaceKey, StringComparison.OrdinalIgnoreCase);
        }

        if (!paceMatches)
            return null;

        var paceScore = paceDelta is null ? 35 : Math.Max(0, 50 - paceDelta.Value);
        var dateScore = Math.Max(0, 25 - (dayDelta * 3));
        var distanceScore = Math.Max(0, 25 - (int)Math.Round(distanceRatio * 70));

        return new MateMatch
        {
            OwnSession = own,
            MateSession = candidate,
            PaceDeltaSeconds = paceDelta,
            DayDelta = dayDelta,
            DistanceDeltaMeters = distanceDelta,
            Score = paceScore + dateScore + distanceScore
        };
    }

    private bool IsCandidate(string athleteUserId, MateAvailabilityDocument own, MateAvailabilityDocument candidate)
    {
        return IsActiveAndCurrent(candidate)
            && !string.Equals(candidate.AthleteUserId, athleteUserId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(candidate.CourseId, own.CourseId, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(candidate.Id, own.Id, StringComparison.OrdinalIgnoreCase);
    }

    private static RunSnapshot BuildRunSnapshot(RunningSession run, PaceModel? paceModel)
    {
        var relevantSegments = run.Sequence
            .Where(segment => MatchSegmentTypes.Contains(segment.Type))
            .ToList();

        if (relevantSegments.Count == 0)
            relevantSegments = run.Sequence.ToList();

        var paceKey = relevantSegments
            .Where(segment => !string.IsNullOrWhiteSpace(segment.PaceKey))
            .OrderByDescending(segment => segment.Distance)
            .Select(segment => segment.PaceKey!)
            .FirstOrDefault() ?? string.Empty;

        return new RunSnapshot(
            run.TotalDistance,
            paceKey,
            ResolveWeightedPaceSeconds(run, paceModel));
    }

    private static int? ResolveWeightedPaceSeconds(RunningSession run, PaceModel? paceModel)
    {
        if (!HasUsablePaceModel(paceModel))
            return null;

        try
        {
            var resolved = RunningSessionResolver.Resolve(run, paceModel!);
            var segments = resolved.Segments
                .Where(segment => MatchSegmentTypes.Contains(segment.Segment.Type))
                .Where(segment => segment.Pace is not null && segment.Pace.Value > TimeSpan.Zero && segment.Segment.Distance > 0)
                .ToList();

            if (segments.Count == 0)
                return null;

            var distance = segments.Sum(segment => segment.Segment.Distance);
            if (distance <= 0)
                return null;

            var weightedSeconds = segments.Sum(segment =>
                segment.Segment.Distance * segment.Pace!.Value.TotalSeconds) / distance;
            return (int)Math.Round(weightedSeconds);
        }
        catch
        {
            return null;
        }
    }

    private static bool HasUsablePaceModel(PaceModel? paceModel)
    {
        return paceModel is not null
            && (paceModel.Recovery > TimeSpan.Zero
                || paceModel.Easy > TimeSpan.Zero
                || paceModel.Threshold > TimeSpan.Zero
                || paceModel.Intervall > TimeSpan.Zero
                || paceModel.FastIntervall > TimeSpan.Zero);
    }

    private IReadOnlySet<string> GetVisiblePlanIds(CourseDocument course)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        return course.TrainingPlanPublications
            .Where(publication => publication.IsVisibleInCourse(course.Id, now))
            .Select(publication => publication.TrainingPlanId)
            .Where(planId => !string.IsNullOrWhiteSpace(planId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private bool IsActiveAndCurrent(MateAvailabilityDocument availability)
    {
        return availability.IsActive && GetEffectiveStart(availability).Date >= LocalToday;
    }

    private DateTime LocalToday => _timeProvider.GetLocalNow().Date;

    private static DateTime GetEffectiveStart(MateAvailabilityDocument availability)
    {
        return availability.StartsAt ?? availability.SessionDate;
    }

    private static string ResolveDisplayName(AthleteModel? athlete, string athleteUserId)
    {
        var publicUserName = athlete?.PublicProfile?.PublicUserName;
        if (!string.IsNullOrWhiteSpace(publicUserName)
            && !publicUserName.Equals("NA", StringComparison.OrdinalIgnoreCase)
            && !publicUserName.Contains('@'))
        {
            return publicUserName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(athlete?.Name)
            && !athlete.Name.Equals("NA", StringComparison.OrdinalIgnoreCase)
            && !athlete.Name.Contains('@'))
        {
            return athlete.Name.Trim();
        }

        return athleteUserId;
    }

    private static string ShareKey(string courseId, string planId, string sessionId)
    {
        return $"{courseId}|{planId}|{sessionId}";
    }

    private sealed record RunSnapshot(
        int DistanceMeters,
        string PaceKey,
        int? PaceSecondsPerKilometer);
}

public static class MateDocumentIds
{
    public static string Availability(string courseId, string athleteUserId, string planId, string sessionId)
    {
        return string.Join(
            ":",
            "mate-availability",
            Normalize(courseId),
            Normalize(athleteUserId),
            Normalize(planId),
            Normalize(sessionId));
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "na"
            : value.Trim().Replace(":", "-", StringComparison.Ordinal);
    }
}
