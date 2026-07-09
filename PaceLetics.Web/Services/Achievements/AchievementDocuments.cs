using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace PaceLetics.Web.Services.Achievements;

public static class AchievementDocumentTypes
{
    public const string Definition = "achievementDefinition";
    public const string Award = "achievementAward";
    public const string Event = "achievementEvent";
    public const string TrainingSessionCompletion = "trainingSessionCompletion";
}

public static class AchievementRuleTypes
{
    public const string TrainingSessionCompleted = "trainingSessionCompleted";
    public const string TrainingSessionCount = "trainingSessionCount";
    public const string TrainingPlanCompleted = "trainingPlanCompleted";
    public const string WorkoutCompleted = "workoutCompleted";
    public const string WorkoutCount = "workoutCount";
    public const string CourseEventCompleted = "courseEventCompleted";
    public const string CourseCompleted = "courseCompleted";
}

public static class AchievementEventTypes
{
    public const string TrainingSessionCompleted = "training-session.completed";
    public const string WorkoutCompleted = "workout.completed";
    public const string CourseEventCompleted = "course-event.completed";
    public const string CourseCompleted = "course.completed";
}

public sealed class AchievementDefinitionDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = AchievementDocumentIds.DefinitionPartition;
    public string DocumentType { get; set; } = AchievementDocumentTypes.Definition;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
    public int Version { get; set; } = 1;
    public AchievementIconDesignDocument Icon { get; set; } = AchievementIconDesignDocument.Default();
    public List<AchievementRuleDocument> Rules { get; set; } = new();

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
            Id = AchievementDocumentIds.CreateDefinitionId(Title);

        CourseId = AchievementDocumentIds.DefinitionPartition;
        DocumentType = AchievementDocumentTypes.Definition;
        Slug = string.IsNullOrWhiteSpace(Slug) ? Id : Slug.Trim();
        Title = Title?.Trim() ?? string.Empty;
        Description = Description?.Trim() ?? string.Empty;
        Icon ??= AchievementIconDesignDocument.Default();
        Icon.Normalize();
        Rules = Rules
            .Where(rule => !string.IsNullOrWhiteSpace(rule.RuleType))
            .Select(rule =>
            {
                rule.Normalize();
                return rule;
            })
            .ToList();
    }
}

public sealed class AchievementIconDesignDocument
{
    public string MaterialIcon { get; set; } = "EmojiEvents";
    public string BackgroundColor { get; set; } = "#F59E0B";
    public string ForegroundColor { get; set; } = "#111827";
    public string AccentColor { get; set; } = "#FDE68A";
    public string Shape { get; set; } = "circle";

    public static AchievementIconDesignDocument Default()
    {
        return new AchievementIconDesignDocument();
    }

    public AchievementIconDesignDocument Snapshot()
    {
        return new AchievementIconDesignDocument
        {
            MaterialIcon = MaterialIcon,
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            AccentColor = AccentColor,
            Shape = Shape
        };
    }

    public void Normalize()
    {
        MaterialIcon = string.IsNullOrWhiteSpace(MaterialIcon) ? "EmojiEvents" : MaterialIcon.Trim();
        BackgroundColor = NormalizeColor(BackgroundColor, "#F59E0B");
        ForegroundColor = NormalizeColor(ForegroundColor, "#111827");
        AccentColor = NormalizeColor(AccentColor, "#FDE68A");
        Shape = string.Equals(Shape, "square", StringComparison.OrdinalIgnoreCase)
            ? "square"
            : "circle";
    }

    private static string NormalizeColor(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        value = value.Trim();
        return value.StartsWith('#') ? value : $"#{value}";
    }
}

public sealed class AchievementRuleDocument
{
    public string RuleType { get; set; } = AchievementRuleTypes.TrainingSessionCount;
    public string PlanId { get; set; } = string.Empty;
    public List<string> SessionIds { get; set; } = new();
    public string WorkoutId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseEventId { get; set; } = string.Empty;
    public int TargetCount { get; set; } = 1;

    public void Normalize()
    {
        RuleType = RuleType?.Trim() ?? string.Empty;
        PlanId = PlanId?.Trim() ?? string.Empty;
        WorkoutId = WorkoutId?.Trim() ?? string.Empty;
        CourseId = CourseId?.Trim() ?? string.Empty;
        CourseEventId = CourseEventId?.Trim() ?? string.Empty;
        TargetCount = Math.Max(1, TargetCount);
        SessionIds = SessionIds
            .Where(sessionId => !string.IsNullOrWhiteSpace(sessionId))
            .Select(sessionId => sessionId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

public sealed class AchievementEventDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = AchievementDocumentTypes.Event;
    public string AthleteUserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string WorkoutId { get; set; } = string.Empty;
    public string WorkoutName { get; set; } = string.Empty;
    public string CourseIdValue { get; set; } = string.Empty;
    public string CourseEventId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();

    public void Normalize()
    {
        AthleteUserId = AthleteUserId?.Trim() ?? string.Empty;
        EventType = EventType?.Trim() ?? string.Empty;
        PlanId = PlanId?.Trim() ?? string.Empty;
        SessionId = SessionId?.Trim() ?? string.Empty;
        WorkoutId = WorkoutId?.Trim() ?? string.Empty;
        WorkoutName = WorkoutName?.Trim() ?? string.Empty;
        CourseIdValue = CourseIdValue?.Trim() ?? string.Empty;
        CourseEventId = CourseEventId?.Trim() ?? string.Empty;
        CorrelationId = CorrelationId?.Trim() ?? string.Empty;
        CourseId = AchievementDocumentIds.AthleteEventPartition(AthleteUserId);
        DocumentType = AchievementDocumentTypes.Event;

        if (OccurredAt == default)
            OccurredAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(Id))
            Id = AchievementDocumentIds.Event(this);
    }
}

public sealed class TrainingSessionCompletionDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = AchievementDocumentTypes.TrainingSessionCompletion;
    public string AthleteUserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public string CompletedByUserId { get; set; } = string.Empty;
    public string Source { get; set; } = "athlete-checkoff";

    public void Normalize()
    {
        AthleteUserId = AthleteUserId?.Trim() ?? string.Empty;
        PlanId = PlanId?.Trim() ?? string.Empty;
        SessionId = SessionId?.Trim() ?? string.Empty;
        CompletedByUserId = string.IsNullOrWhiteSpace(CompletedByUserId)
            ? AthleteUserId
            : CompletedByUserId.Trim();
        Source = string.IsNullOrWhiteSpace(Source) ? "athlete-checkoff" : Source.Trim();
        CourseId = AchievementDocumentIds.AthleteCompletionPartition(AthleteUserId);
        DocumentType = AchievementDocumentTypes.TrainingSessionCompletion;
        Id = AchievementDocumentIds.TrainingSessionCompletion(AthleteUserId, PlanId, SessionId);

        if (CompletedAt == default)
            CompletedAt = DateTime.UtcNow;
    }
}

public sealed class AthleteAchievementDocument : IQueryItem
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = AchievementDocumentTypes.Award;
    public string AthleteUserId { get; set; } = string.Empty;
    public string AchievementDefinitionId { get; set; } = string.Empty;
    public int DefinitionVersion { get; set; }
    public DateTime AwardedAt { get; set; }
    public string TitleSnapshot { get; set; } = string.Empty;
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public AchievementIconDesignDocument IconSnapshot { get; set; } = AchievementIconDesignDocument.Default();
    public List<string> TriggerEventIds { get; set; } = new();

    public void Normalize()
    {
        AthleteUserId = AthleteUserId?.Trim() ?? string.Empty;
        AchievementDefinitionId = AchievementDefinitionId?.Trim() ?? string.Empty;
        CourseId = AchievementDocumentIds.AthleteAwardPartition(AthleteUserId);
        DocumentType = AchievementDocumentTypes.Award;
        Id = AchievementDocumentIds.Award(AthleteUserId, AchievementDefinitionId);
        TitleSnapshot = TitleSnapshot?.Trim() ?? string.Empty;
        DescriptionSnapshot = DescriptionSnapshot?.Trim() ?? string.Empty;
        IconSnapshot ??= AchievementIconDesignDocument.Default();
        IconSnapshot.Normalize();
        TriggerEventIds = TriggerEventIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (AwardedAt == default)
            AwardedAt = DateTime.UtcNow;
    }
}

public sealed class AchievementDefinitionRequest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public AchievementIconDesignDocument Icon { get; set; } = AchievementIconDesignDocument.Default();
    public List<AchievementRuleDocument> Rules { get; set; } = new();
}

public sealed class AchievementEvaluationResult
{
    public AchievementEvaluationResult(IReadOnlyList<AthleteAchievementDocument> awarded)
    {
        Awarded = awarded;
    }

    public IReadOnlyList<AthleteAchievementDocument> Awarded { get; }
}

public static class AchievementDocumentIds
{
    public const string DefinitionPartition = "achievement-definitions";

    public static string CreateDefinitionId(string title)
    {
        var slug = ToSlug(title);
        if (slug.Length > 32)
            slug = slug[..32];

        return $"achievement:{slug}:{Guid.NewGuid():N}";
    }

    public static string AthleteAwardPartition(string athleteUserId)
    {
        return $"achievement-awards:{Normalize(athleteUserId)}";
    }

    public static string AthleteEventPartition(string athleteUserId)
    {
        return $"achievement-events:{Normalize(athleteUserId)}";
    }

    public static string AthleteCompletionPartition(string athleteUserId)
    {
        return $"training-session-completions:{Normalize(athleteUserId)}";
    }

    public static string Award(string athleteUserId, string achievementDefinitionId)
    {
        return $"achievement-award:{Normalize(achievementDefinitionId)}:{Normalize(athleteUserId)}";
    }

    public static string TrainingSessionCompletion(string athleteUserId, string planId, string sessionId)
    {
        return $"training-session-completion:{Normalize(athleteUserId)}:{Normalize(planId)}:{Normalize(sessionId)}";
    }

    public static string Event(AchievementEventDocument achievementEvent)
    {
        if (!string.IsNullOrWhiteSpace(achievementEvent.CorrelationId))
        {
            return $"achievement-event:{Normalize(achievementEvent.EventType)}:{Normalize(achievementEvent.AthleteUserId)}:{Normalize(achievementEvent.CorrelationId)}";
        }

        return $"achievement-event:{Normalize(achievementEvent.EventType)}:{Normalize(achievementEvent.AthleteUserId)}:{Guid.NewGuid():N}";
    }

    public static string TrainingSessionCorrelation(string planId, string sessionId)
    {
        return $"training-session:{Normalize(planId)}:{Normalize(sessionId)}";
    }

    private static string ToSlug(string value)
    {
        var characters = (value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        var slug = string.Join("-", new string(characters).Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? "achievement" : slug;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "empty"
            : value.Trim().Replace(":", "-", StringComparison.Ordinal);
    }
}
