namespace PaceLetics.Web.Services.Workouts;

public sealed class WorkoutCatalogStartupLoader : IHostedService
{
    private readonly WorkoutCatalogManagementService _catalogService;
    private readonly ILogger<WorkoutCatalogStartupLoader> _logger;

    public WorkoutCatalogStartupLoader(
        WorkoutCatalogManagementService catalogService,
        ILogger<WorkoutCatalogStartupLoader> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = LoadCatalogAsync();
        return Task.CompletedTask;
    }

    private async Task LoadCatalogAsync()
    {
        try
        {
            await _catalogService.EnsureLoadedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not load the persisted workout catalog. Continuing with the JSON seed catalog.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
