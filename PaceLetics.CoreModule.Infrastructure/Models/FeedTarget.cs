namespace PaceLetics.CoreModule.Infrastructure.Models;

public static class FeedTargetTypes
{
    public const string Global = "global";
    public const string User = "user";
    public const string Course = "course";
    public const string Team = "team";
    public const string Organization = "organization";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Global,
        User,
        Course,
        Team,
        Organization
    };
}

public sealed class FeedTarget : IEquatable<FeedTarget>
{
    public string TargetType { get; set; } = FeedTargetTypes.Global;

    public string TargetId { get; set; } = string.Empty;

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(TargetType)
        && string.IsNullOrWhiteSpace(TargetId);

    public bool IsGlobal =>
        string.Equals(TargetType, FeedTargetTypes.Global, StringComparison.OrdinalIgnoreCase);

    public static FeedTarget Global()
    {
        return new FeedTarget
        {
            TargetType = FeedTargetTypes.Global,
            TargetId = string.Empty
        };
    }

    public static FeedTarget User(string userId)
    {
        return Addressed(FeedTargetTypes.User, userId);
    }

    public static FeedTarget Course(string courseId)
    {
        return Addressed(FeedTargetTypes.Course, courseId);
    }

    public static FeedTarget Team(string teamId)
    {
        return Addressed(FeedTargetTypes.Team, teamId);
    }

    public static FeedTarget Organization(string organizationId)
    {
        return Addressed(FeedTargetTypes.Organization, organizationId);
    }

    public FeedTarget NormalizeCopy()
    {
        var targetType = NormalizeTargetType(TargetType);
        var targetId = TargetId?.Trim() ?? string.Empty;

        return new FeedTarget
        {
            TargetType = targetType,
            TargetId = string.Equals(targetType, FeedTargetTypes.Global, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : targetId
        };
    }

    public void Validate()
    {
        var normalized = NormalizeCopy();

        if (!FeedTargetTypes.All.Contains(normalized.TargetType))
            throw new InvalidOperationException($"Unsupported feed target type '{TargetType}'.");

        if (!normalized.IsGlobal && string.IsNullOrWhiteSpace(normalized.TargetId))
            throw new InvalidOperationException($"Feed target '{normalized.TargetType}' requires an id.");
    }

    public bool Matches(FeedTarget other)
    {
        if (other is null)
            return false;

        var left = NormalizeCopy();
        var right = other.NormalizeCopy();

        return string.Equals(left.TargetType, right.TargetType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.TargetId, right.TargetId, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(FeedTarget? other)
    {
        return other is not null && Matches(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is FeedTarget other && Equals(other);
    }

    public override int GetHashCode()
    {
        var normalized = NormalizeCopy();
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(normalized.TargetType),
            StringComparer.OrdinalIgnoreCase.GetHashCode(normalized.TargetId));
    }

    private static FeedTarget Addressed(string targetType, string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("A feed target id is required.", nameof(targetId));

        return new FeedTarget
        {
            TargetType = targetType,
            TargetId = targetId.Trim()
        };
    }

    private static string NormalizeTargetType(string? targetType)
    {
        var value = targetType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        foreach (var knownType in FeedTargetTypes.All)
        {
            if (string.Equals(value, knownType, StringComparison.OrdinalIgnoreCase))
                return knownType;
        }

        return value;
    }
}
