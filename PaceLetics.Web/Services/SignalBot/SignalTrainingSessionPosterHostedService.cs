using Microsoft.Extensions.Options;

namespace PaceLetics.Web.Services.SignalBot;

public sealed class SignalTrainingSessionPosterHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<SignalBotOptions> _options;
    private readonly ILogger<SignalTrainingSessionPosterHostedService> _logger;
    private DateTime? _lastPostDate;

    public SignalTrainingSessionPosterHostedService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<SignalBotOptions> options,
        ILogger<SignalTrainingSessionPosterHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var options = _options.CurrentValue;
            if (!options.Enabled || !options.PostAutomatically)
                continue;

            var localNow = GetLocalNow(options.TimeZoneId);
            if (_lastPostDate == localNow.Date || !ShouldPostNow(localNow, options.DailyPostTime))
                continue;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notifier = scope.ServiceProvider.GetRequiredService<ISignalTrainingSessionNotifier>();
                var result = await notifier.PostCurrentTrainingSessionsAsync(stoppingToken);

                if (result.Success)
                    _lastPostDate = localNow.Date;

                if (!result.Success)
                    _logger.LogWarning("Signal training session post skipped: {Reason}", result.Reason);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signal training session post failed.");
            }
        }
    }

    private static bool ShouldPostNow(DateTimeOffset localNow, string dailyPostTime)
    {
        return TimeSpan.TryParse(dailyPostTime, out var postTime)
            && localNow.TimeOfDay >= postTime;
    }

    private static DateTimeOffset GetLocalNow(string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateTimeOffset.UtcNow;
        }
        catch (InvalidTimeZoneException)
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
