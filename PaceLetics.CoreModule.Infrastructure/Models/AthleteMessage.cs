namespace PaceLetics.CoreModule.Infrastructure.Models;

public sealed record AthleteMessage(
    string Id,
    string SourceModule,
    AthleteMessageSeverity Severity,
    string TitleKey,
    string BodyKey,
    string Icon,
    string? ActionHref,
    string? ActionLabelKey,
    int Priority,
    IReadOnlyList<object> BodyArguments);

public enum AthleteMessageSeverity
{
    Info,
    Success,
    Warning,
    Error
}
