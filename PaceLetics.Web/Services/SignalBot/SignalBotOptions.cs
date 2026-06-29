namespace PaceLetics.Web.Services.SignalBot;

public sealed class SignalBotOptions
{
    public const string SectionName = "SignalBot";

    public bool Enabled { get; set; }
    public bool PostAutomatically { get; set; } = true;
    public bool PostWhenEmpty { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string SenderNumber { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public List<string> AthleteUserIds { get; set; } = new();
    public int LookAheadDays { get; set; }
    public string DailyPostTime { get; set; } = "07:00";
    public string TimeZoneId { get; set; } = "Europe/Berlin";
}
