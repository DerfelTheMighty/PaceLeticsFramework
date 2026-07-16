namespace PaceLetics.Web.Services.Integrations.Strava;

public interface IStravaIntegrationService
{
    bool IsConfigured { get; }
    string CreateAuthorizationUrl(string athleteUserId, string redirectUri, string protectedState);
    Task CompleteAuthorizationAsync(string athleteUserId, string code, string grantedScope, CancellationToken cancellationToken = default);
    Task<StravaConnectionStatus> GetStatusAsync(string athleteUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StravaActivityDocument>> GetActivitiesAsync(string athleteUserId, CancellationToken cancellationToken = default);
    Task<StravaSyncResult> SyncAsync(string athleteUserId, CancellationToken cancellationToken = default);
    Task DisconnectAsync(string athleteUserId, CancellationToken cancellationToken = default);
}
