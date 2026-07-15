namespace PaceLetics.Web.Services.Courses;

public sealed class CourseSeedStartupLoader : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CourseSeedStartupLoader> _logger;

    public CourseSeedStartupLoader(
        IServiceScopeFactory scopeFactory,
        ILogger<CourseSeedStartupLoader> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICourseRepository>();
            await repository.GetCoursesAsync();
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Could not ensure the default courses in Cosmos DB.");
        }
    }
}
