using MudBlazor;
namespace PaceLetics.CoreModule.Infrastructure.Models;

public sealed record AthleteMessage(
    string Id,
    string SourceModule,
    Severity Severity,
    string TitleKey,
    string BodyKey,
    string Icon,
    string? ActionHref,
    string? ActionLabelKey,
    int Priority,
    IReadOnlyList<object> BodyArguments,
    bool IsRead = false)
{
    public bool IsUnread => !IsRead;
}

