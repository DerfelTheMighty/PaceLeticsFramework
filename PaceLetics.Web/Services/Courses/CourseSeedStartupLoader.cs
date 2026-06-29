namespace PaceLetics.Web.Services.Courses;

public sealed class CourseSeedStartupLoader : IHostedService
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = EnsureCoursesAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task EnsureCoursesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICourseRepository>();
            await repository.GetCoursesAsync();
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Could not ensure the default courses in Cosmos DB.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
