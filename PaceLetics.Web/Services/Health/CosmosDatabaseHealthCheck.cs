using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PaceLetics.Web.Services.Health;

public sealed class CosmosDatabaseHealthCheck : IHealthCheck
{
    private readonly CosmosClient _client;

    public CosmosDatabaseHealthCheck(CosmosClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.ReadAccountAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB is unavailable.", ex);
        }
    }
}
