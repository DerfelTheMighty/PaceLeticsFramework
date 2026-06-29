using System.Globalization;
using AthleteDataAccessLibrary.Contracts;
using Microsoft.Extensions.Options;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.Web.Services.Calendar;

namespace PaceLetics.Web.Services.SignalBot;

public sealed class SignalTrainingSessionNotifier : ISignalTrainingSessionNotifier
{
    private readonly ITrainingCalendarService _trainingCalendarService;
    private readonly IAthleteData _athleteData;
    private readonly ISignalMessageClient _signalMessageClient;
    private readonly IOptionsMonitor<SignalBotOptions> _options;
    private readonly ILogger<SignalTrainingSessionNotifier> _logger;

    public SignalTrainingSessionNotifier(
        ITrainingCalendarService trainingCalendarService,
        IAthleteData athleteData,
        ISignalMessageClient signalMessageClient,
        IOptionsMonitor<SignalBotOptions> options,
        ILogger<SignalTrainingSessionNotifier> logger)
    {
        _trainingCalendarService = trainingCalendarService;
        _athleteData = athleteData;
        _signalMessageClient = signalMessageClient;
        _options = options;
        _logger = logger;
    }

    public async Task<SignalBotPostResult> PostCurrentTrainingSessionsAsync(CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        var validationError = Validate(options);
        if (validationError is not null)
            return new SignalBotPostResult(false, false, 0, string.Empty, validationError);

        var timeZone = ResolveTimeZone(options.TimeZoneId);
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        var startDate = localNow.Date;
        var endDate = startDate.AddDays(Math.Max(0, options.LookAheadDays) + 1);
        var athleteDisplayNames = await LoadAthleteDisplayNamesAsync(options.AthleteUserIds, cancellationToken);
        var sessionLines = new List<string>();

        foreach (var athleteUserId in options.AthleteUserIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var items = await _trainingCalendarService.GetCalendarItemsForAthleteAsync(athleteUserId);
            var sessions = items
                .Where(item => item.Kind == TrainingCalendarItemKinds.TrainingSession)
                .Where(item => item.StartsAt.Date >= startDate && item.StartsAt.Date < endDate)
                .OrderBy(item => item.StartsAt)
                .ThenBy(item => item.Title)
                .ToList();

            if (sessions.Count == 0)
                continue;

            sessionLines.Add(ResolveDisplayName(athleteDisplayNames, athleteUserId));
            sessionLines.AddRange(sessions.Select(FormatSession));
            sessionLines.Add(string.Empty);
        }

        var sessionCount = sessionLines.Count(line => line.StartsWith("- ", StringComparison.Ordinal));
        if (sessionCount == 0 && !options.PostWhenEmpty)
        {
            return new SignalBotPostResult(true, false, 0, string.Empty, "No current training sessions found.");
        }

        var message = BuildMessage(startDate, endDate.AddDays(-1), sessionLines, sessionCount);
        await _signalMessageClient.SendAsync(
            options.SenderNumber.Trim(),
            options.Recipients.Select(recipient => recipient.Trim()).Where(recipient => recipient.Length > 0).ToList(),
            message,
            cancellationToken);

        _logger.LogInformation("Posted {SessionCount} current training sessions to Signal.", sessionCount);
        return new SignalBotPostResult(true, true, sessionCount, message);
    }

    private static string? Validate(SignalBotOptions options)
    {
        if (!options.Enabled)
            return "Signal bot is disabled.";

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return "SignalBot:BaseUrl is required.";

        if (string.IsNullOrWhiteSpace(options.SenderNumber))
            return "SignalBot:SenderNumber is required.";

        if (options.Recipients.Count == 0 || options.Recipients.All(string.IsNullOrWhiteSpace))
            return "SignalBot:Recipients must contain at least one Signal recipient.";

        if (options.AthleteUserIds.Count == 0 || options.AthleteUserIds.All(string.IsNullOrWhiteSpace))
            return "SignalBot:AthleteUserIds must contain at least one athlete user id.";

        return null;
    }

    private async Task<Dictionary<string, string>> LoadAthleteDisplayNamesAsync(
        IEnumerable<string> athleteUserIds,
        CancellationToken cancellationToken)
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var athleteUserId in athleteUserIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var athlete = await _athleteData.GetAthlete(athleteUserId);
                names[athleteUserId] = GetDisplayName(athlete, athleteUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load athlete display name for {AthleteUserId}.", athleteUserId);
                names[athleteUserId] = athleteUserId;
            }
        }

        return names;
    }

    private static string ResolveDisplayName(IReadOnlyDictionary<string, string> names, string athleteUserId)
    {
        return names.TryGetValue(athleteUserId, out var displayName)
            ? displayName
            : athleteUserId;
    }

    private static string GetDisplayName(AthleteModel? athlete, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(athlete?.PublicProfile?.PublicUserName)
            && athlete.PublicProfile.PublicUserName != "NA")
        {
            return athlete.PublicProfile.PublicUserName;
        }

        if (!string.IsNullOrWhiteSpace(athlete?.Name) && athlete.Name != "NA")
            return athlete.Name;

        return fallback;
    }

    private static string FormatSession(TrainingCalendarItem item)
    {
        var details = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.PlanName))
            details.Add(item.PlanName);

        if (!string.IsNullOrWhiteSpace(item.CourseName))
            details.Add(item.CourseName);

        if (!string.IsNullOrWhiteSpace(item.Description))
            details.Add(item.Description);

        if (!string.IsNullOrWhiteSpace(item.Location))
            details.Add(item.Location);

        var when = item.HasTime
            ? item.StartsAt.ToString("dd.MM. HH:mm", CultureInfo.GetCultureInfo("de-DE"))
            : item.StartsAt.ToString("dd.MM.", CultureInfo.GetCultureInfo("de-DE"));
        var detailText = details.Count > 0 ? $" ({string.Join(" - ", details)})" : string.Empty;

        return $"- {when} {item.Title}{detailText}";
    }

    private static string BuildMessage(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<string> sessionLines,
        int sessionCount)
    {
        var dateText = startDate == endDate
            ? startDate.ToString("dddd, dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"))
            : $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";

        var lines = new List<string>
        {
            "PaceLetics Trainingssessions",
            dateText,
            string.Empty
        };

        if (sessionCount == 0)
        {
            lines.Add("Keine aktuellen Trainingssessions gefunden.");
        }
        else
        {
            lines.AddRange(sessionLines);
        }

        return string.Join(Environment.NewLine, lines).TrimEnd();
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
