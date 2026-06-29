namespace PaceLetics.Web.Services.SignalBot;

public sealed record SignalBotPostResult(
    bool Success,
    bool Sent,
    int SessionCount,
    string Message,
    string? Reason = null);
