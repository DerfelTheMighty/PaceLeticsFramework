namespace PaceLetics.Web.Services.Workouts;

public sealed class WorkoutCatalogStartupLoader : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

}
