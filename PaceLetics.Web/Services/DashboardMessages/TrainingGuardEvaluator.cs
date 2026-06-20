using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.TrainingModule.CodeBase.Running.Models;
using PaceLetics.TrainingPlanModule.CodeBase.Models;

namespace PaceLetics.Web.Services.DashboardMessages;

public sealed class TrainingGuardEvaluator
{
    public IReadOnlyList<TrainingGuardFinding> Evaluate(
        AthleteMessageContext context,
        IReadOnlyList<TrainingPlan> plans,
        DashboardMessageFeedOptions options)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(plans);
        ArgumentNullException.ThrowIfNull(options);

        var session = FindUpcomingSession(context, plans);
        if (session is null)
            return Array.Empty<TrainingGuardFinding>();

        var daysUntil = (session.Date.Date - context.Today.Date).Days;
        if (daysUntil < 0 || daysUntil > options.TrainingGuardLookAhead.TotalDays)
            return Array.Empty<TrainingGuardFinding>();

        if (!IsKeySession(session))
            return Array.Empty<TrainingGuardFinding>();

        var findings = new List<TrainingGuardFinding>();

        if (HasStaleUsableReference(context.ActiveReferenceResult, context.Today, options.ReferenceRunMaxAge, out var referenceAgeDays))
        {
            findings.Add(new TrainingGuardFinding(
                TrainingGuardFindingType.StaleReferenceBeforeKeySession,
                session,
                daysUntil,
                referenceAgeDays));
        }

        if (!HasWarmup(session))
        {
            findings.Add(new TrainingGuardFinding(
                TrainingGuardFindingType.MissingWarmupBeforeKeySession,
                session,
                daysUntil));
        }

        return findings;
    }

    private static TrainingSession? FindUpcomingSession(
        AthleteMessageContext context,
        IReadOnlyList<TrainingPlan> plans)
    {
        if (plans.Count == 0)
            return null;

        var selectedPlan = string.IsNullOrWhiteSpace(context.SelectedTrainingPlanId)
            ? null
            : plans.FirstOrDefault(plan => string.Equals(plan.Id, context.SelectedTrainingPlanId, StringComparison.OrdinalIgnoreCase));

        return selectedPlan is not null
            ? FindNextSession(selectedPlan, context.Today)
            : plans
                .Select(plan => FindNextSession(plan, context.Today))
                .Where(session => session is not null)
                .OrderBy(session => session!.Date)
                .FirstOrDefault();
    }

    private static TrainingSession? FindNextSession(TrainingPlan plan, DateTime today)
    {
        return plan.Sessions
            .Where(session => session.Date.Date >= today.Date)
            .OrderBy(session => session.Date)
            .FirstOrDefault();
    }

    private static bool IsKeySession(TrainingSession session)
    {
        return session.Runs.Any(run => run.Sequence.Any(IsQualitySegment));
    }

    private static bool IsQualitySegment(RunningSegment segment)
    {
        return segment.Type == SegmentType.Intervall
            || string.Equals(segment.PaceKey, PaceKeys.Threshold, StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment.PaceKey, PaceKeys.Intervall, StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment.PaceKey, PaceKeys.Repetition, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasWarmup(TrainingSession session)
    {
        return session.Warmup.Count > 0
            || session.Runs.Any(run =>
                run.WarmupDistance.GetValueOrDefault() > 0
                || run.Sequence.Any(segment => segment.Type == SegmentType.Warmup));
    }

    private static bool HasStaleUsableReference(
        RaceResultModel? referenceResult,
        DateTime today,
        TimeSpan maxAge,
        out int ageDays)
    {
        ageDays = 0;
        if (referenceResult is null
            || referenceResult.DistanceM <= 0
            || referenceResult.Time <= TimeSpan.Zero)
        {
            return false;
        }

        ageDays = (int)Math.Floor((today.Date - referenceResult.Date.Date).TotalDays);
        return ageDays > maxAge.TotalDays;
    }
}

public sealed record TrainingGuardFinding(
    TrainingGuardFindingType Type,
    TrainingSession Session,
    int DaysUntil,
    int? ReferenceAgeDays = null);

public enum TrainingGuardFindingType
{
    StaleReferenceBeforeKeySession,
    MissingWarmupBeforeKeySession
}
