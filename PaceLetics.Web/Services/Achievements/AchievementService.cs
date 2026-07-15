using PaceLetics.TrainingPlanModule.CodeBase.Models;
using PaceLetics.Web.Services;

namespace PaceLetics.Web.Services.Achievements;

public sealed class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _repository;
    private readonly ITrainingPlanService? _trainingPlanService;
    private readonly TimeProvider _timeProvider;

    public AchievementService(
        IAchievementRepository repository,
        ITrainingPlanService? trainingPlanService = null,
        TimeProvider? timeProvider = null)
    {
        _repository = repository;
        _trainingPlanService = trainingPlanService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<IReadOnlyList<AchievementDefinitionDocument>> GetDefinitionsAsync(bool includeUnpublished = false)
    {
        var definitions = await _repository.GetDefinitionsAsync();
        return definitions
            .Where(definition => includeUnpublished || definition.IsPublished)
            .OrderBy(definition => definition.Title)
            .ToList();
    }

    public async Task<AchievementDefinitionDocument?> GetDefinitionAsync(string achievementDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(achievementDefinitionId))
            return null;

        var definition = await _repository.GetDefinitionAsync(achievementDefinitionId);
        definition?.Normalize();
        return definition;
    }

    public async Task<AchievementDefinitionDocument> SaveDefinitionAsync(
        AchievementDefinitionRequest request,
        string requestingUserId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(requestingUserId))
            throw new InvalidOperationException("Zum Speichern eines Achievements ist eine Anmeldung erforderlich.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidOperationException("Ein Achievement braucht einen Titel.");

        if (request.Rules.Count == 0)
            throw new InvalidOperationException("Ein Achievement braucht mindestens eine Regel.");

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var existing = string.IsNullOrWhiteSpace(request.Id)
            ? null
            : await _repository.GetDefinitionAsync(request.Id);

        var definition = existing ?? new AchievementDefinitionDocument
        {
            Id = AchievementDocumentIds.CreateDefinitionId(request.Title),
            CreatedAt = now,
            CreatedByUserId = requestingUserId,
            Version = 0
        };

        definition.Title = request.Title;
        definition.Description = request.Description;
        definition.IsPublished = request.IsPublished;
        definition.Icon = request.Icon?.Snapshot() ?? AchievementIconDesignDocument.Default();
        definition.Rules = request.Rules
            .Select(rule => new AchievementRuleDocument
            {
                RuleType = rule.RuleType,
                PlanId = rule.PlanId,
                SessionIds = rule.SessionIds.ToList(),
                WorkoutId = rule.WorkoutId,
                CourseId = rule.CourseId,
                CourseEventId = rule.CourseEventId,
                TargetCount = rule.TargetCount
            })
            .ToList();
        definition.UpdatedAt = now;
        definition.Version = Math.Max(1, definition.Version + 1);
        definition.Normalize();

        await _repository.UpsertDefinitionAsync(definition);
        return definition;
    }

    public async Task DeleteDefinitionAsync(string achievementDefinitionId, string requestingUserId)
    {
        if (string.IsNullOrWhiteSpace(requestingUserId))
            throw new InvalidOperationException("Zum Löschen eines Achievements ist eine Anmeldung erforderlich.");

        var definition = await _repository.GetDefinitionAsync(achievementDefinitionId)
            ?? throw new InvalidOperationException("Das Achievement wurde nicht gefunden.");

        if (!string.Equals(definition.CreatedByUserId, requestingUserId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Nur die erstellende Person kann dieses Achievement löschen.");

        await _repository.DeleteDefinitionAsync(achievementDefinitionId);
    }

    public Task<IReadOnlyList<AthleteAchievementDocument>> GetAwardsForAthleteAsync(string athleteUserId)
    {
        return _repository.GetAwardsForAthleteAsync(athleteUserId);
    }

    public Task<IReadOnlyList<TrainingSessionCompletionDocument>> GetTrainingSessionCompletionsForAthleteAsync(string athleteUserId)
    {
        return _repository.GetTrainingSessionCompletionsForAthleteAsync(athleteUserId);
    }

    public async Task<AchievementEvaluationResult> SetTrainingSessionCompletionAsync(
        string athleteUserId,
        string planId,
        string sessionId,
        bool isCompleted)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId)
            || string.IsNullOrWhiteSpace(planId)
            || string.IsNullOrWhiteSpace(sessionId))
        {
            return new AchievementEvaluationResult(Array.Empty<AthleteAchievementDocument>());
        }

        var existing = await _repository.GetTrainingSessionCompletionAsync(athleteUserId, planId, sessionId);
        if (!isCompleted)
        {
            if (existing is not null)
                await _repository.DeleteTrainingSessionCompletionAsync(athleteUserId, planId, sessionId);

            return new AchievementEvaluationResult(Array.Empty<AthleteAchievementDocument>());
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var completion = existing ?? new TrainingSessionCompletionDocument
        {
            AthleteUserId = athleteUserId,
            PlanId = planId,
            SessionId = sessionId,
            CompletedAt = now,
            CompletedByUserId = athleteUserId
        };

        completion.Normalize();
        await _repository.UpsertTrainingSessionCompletionAsync(completion);

        var achievementEvent = new AchievementEventDocument
        {
            AthleteUserId = athleteUserId,
            EventType = AchievementEventTypes.TrainingSessionCompleted,
            OccurredAt = completion.CompletedAt,
            PlanId = planId,
            SessionId = sessionId,
            CorrelationId = AchievementDocumentIds.TrainingSessionCorrelation(planId, sessionId)
        };
        achievementEvent.Normalize();
        await _repository.UpsertEventAsync(achievementEvent);

        return await EvaluateAthleteAsync(athleteUserId, achievementEvent);
    }

    public async Task<AchievementEvaluationResult> RecordWorkoutCompletedAsync(
        string athleteUserId,
        string workoutId,
        string workoutName)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return new AchievementEvaluationResult(Array.Empty<AthleteAchievementDocument>());

        var achievementEvent = new AchievementEventDocument
        {
            AthleteUserId = athleteUserId,
            EventType = AchievementEventTypes.WorkoutCompleted,
            OccurredAt = _timeProvider.GetUtcNow().UtcDateTime,
            WorkoutId = workoutId,
            WorkoutName = workoutName
        };
        achievementEvent.Normalize();

        await _repository.UpsertEventAsync(achievementEvent);
        return await EvaluateAthleteAsync(athleteUserId, achievementEvent);
    }

    public Task<AchievementEvaluationResult> EvaluateAthleteAsync(string athleteUserId)
    {
        return EvaluateAthleteAsync(athleteUserId, null);
    }

    private async Task<AchievementEvaluationResult> EvaluateAthleteAsync(
        string athleteUserId,
        AchievementEventDocument? triggerEvent)
    {
        if (string.IsNullOrWhiteSpace(athleteUserId))
            return new AchievementEvaluationResult(Array.Empty<AthleteAchievementDocument>());

        var definitions = await GetDefinitionsAsync();
        if (definitions.Count == 0)
            return new AchievementEvaluationResult(Array.Empty<AthleteAchievementDocument>());

        var awards = await _repository.GetAwardsForAthleteAsync(athleteUserId);
        var awardedDefinitionIds = awards
            .Select(award => award.AchievementDefinitionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var events = await _repository.GetEventsForAthleteAsync(athleteUserId);
        var completions = await _repository.GetTrainingSessionCompletionsForAthleteAsync(athleteUserId);
        var plans = await LoadVisiblePlansAsync(athleteUserId);
        var newlyAwarded = new List<AthleteAchievementDocument>();

        foreach (var definition in definitions)
        {
            if (awardedDefinitionIds.Contains(definition.Id))
                continue;

            if (!IsDefinitionSatisfied(definition, events, completions, plans))
                continue;

            var award = new AthleteAchievementDocument
            {
                AthleteUserId = athleteUserId,
                AchievementDefinitionId = definition.Id,
                DefinitionVersion = definition.Version,
                AwardedAt = _timeProvider.GetUtcNow().UtcDateTime,
                TitleSnapshot = definition.Title,
                DescriptionSnapshot = definition.Description,
                IconSnapshot = definition.Icon.Snapshot(),
                TriggerEventIds = triggerEvent is null
                    ? Array.Empty<string>().ToList()
                    : new List<string> { triggerEvent.Id }
            };
            award.Normalize();

            await _repository.UpsertAwardAsync(award);
            awardedDefinitionIds.Add(definition.Id);
            newlyAwarded.Add(award);
        }

        return new AchievementEvaluationResult(newlyAwarded);
    }

    private async Task<IReadOnlyList<TrainingPlan>> LoadVisiblePlansAsync(string athleteUserId)
    {
        if (_trainingPlanService is null)
            return Array.Empty<TrainingPlan>();

        try
        {
            return await _trainingPlanService.LoadTrainingPlansForUserAsync(athleteUserId);
        }
        catch
        {
            return Array.Empty<TrainingPlan>();
        }
    }

    private static bool IsDefinitionSatisfied(
        AchievementDefinitionDocument definition,
        IReadOnlyList<AchievementEventDocument> events,
        IReadOnlyList<TrainingSessionCompletionDocument> completions,
        IReadOnlyList<TrainingPlan> plans)
    {
        definition.Normalize();
        if (definition.Rules.Count == 0)
            return false;

        return definition.Rules.All(rule => IsRuleSatisfied(rule, events, completions, plans));
    }

    private static bool IsRuleSatisfied(
        AchievementRuleDocument rule,
        IReadOnlyList<AchievementEventDocument> events,
        IReadOnlyList<TrainingSessionCompletionDocument> completions,
        IReadOnlyList<TrainingPlan> plans)
    {
        rule.Normalize();

        return rule.RuleType switch
        {
            AchievementRuleTypes.TrainingSessionCompleted => IsTrainingSessionCompletedRuleSatisfied(rule, completions),
            AchievementRuleTypes.TrainingSessionCount => CountTrainingSessionCompletions(rule, completions) >= rule.TargetCount,
            AchievementRuleTypes.TrainingPlanCompleted => IsTrainingPlanCompletedRuleSatisfied(rule, completions, plans),
            AchievementRuleTypes.WorkoutCompleted => CountWorkoutEvents(rule, events) >= 1,
            AchievementRuleTypes.WorkoutCount => CountWorkoutEvents(rule, events) >= rule.TargetCount,
            AchievementRuleTypes.CourseEventCompleted => CountCourseEventCompletions(rule, events) >= 1,
            AchievementRuleTypes.CourseCompleted => CountCourseCompletions(rule, events) >= 1,
            _ => false
        };
    }

    private static bool IsTrainingSessionCompletedRuleSatisfied(
        AchievementRuleDocument rule,
        IReadOnlyList<TrainingSessionCompletionDocument> completions)
    {
        var matchingCompletions = completions
            .Where(completion => Matches(rule.PlanId, completion.PlanId))
            .ToList();

        if (rule.SessionIds.Count == 0)
            return matchingCompletions.Any();

        var completedSessionIds = matchingCompletions
            .Select(completion => completion.SessionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return rule.SessionIds.All(completedSessionIds.Contains);
    }

    private static int CountTrainingSessionCompletions(
        AchievementRuleDocument rule,
        IReadOnlyList<TrainingSessionCompletionDocument> completions)
    {
        return completions.Count(completion => Matches(rule.PlanId, completion.PlanId));
    }

    private static bool IsTrainingPlanCompletedRuleSatisfied(
        AchievementRuleDocument rule,
        IReadOnlyList<TrainingSessionCompletionDocument> completions,
        IReadOnlyList<TrainingPlan> plans)
    {
        if (string.IsNullOrWhiteSpace(rule.PlanId))
            return false;

        var plan = plans.FirstOrDefault(plan => string.Equals(plan.Id, rule.PlanId, StringComparison.OrdinalIgnoreCase));
        if (plan is null || plan.Sessions.Count == 0)
            return false;

        var completedSessionIds = completions
            .Where(completion => string.Equals(completion.PlanId, rule.PlanId, StringComparison.OrdinalIgnoreCase))
            .Select(completion => completion.SessionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return plan.Sessions.All(session => completedSessionIds.Contains(session.Id));
    }

    private static int CountWorkoutEvents(
        AchievementRuleDocument rule,
        IReadOnlyList<AchievementEventDocument> events)
    {
        return events.Count(achievementEvent =>
            string.Equals(achievementEvent.EventType, AchievementEventTypes.WorkoutCompleted, StringComparison.OrdinalIgnoreCase)
            && Matches(rule.WorkoutId, achievementEvent.WorkoutId));
    }

    private static int CountCourseEventCompletions(
        AchievementRuleDocument rule,
        IReadOnlyList<AchievementEventDocument> events)
    {
        return events.Count(achievementEvent =>
            string.Equals(achievementEvent.EventType, AchievementEventTypes.CourseEventCompleted, StringComparison.OrdinalIgnoreCase)
            && Matches(rule.CourseId, achievementEvent.CourseIdValue)
            && Matches(rule.CourseEventId, achievementEvent.CourseEventId));
    }

    private static int CountCourseCompletions(
        AchievementRuleDocument rule,
        IReadOnlyList<AchievementEventDocument> events)
    {
        return events.Count(achievementEvent =>
            string.Equals(achievementEvent.EventType, AchievementEventTypes.CourseCompleted, StringComparison.OrdinalIgnoreCase)
            && Matches(rule.CourseId, achievementEvent.CourseIdValue));
    }

    private static bool Matches(string expected, string actual)
    {
        return string.IsNullOrWhiteSpace(expected)
            || string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
    }
}
