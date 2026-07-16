namespace PaceLetics.Web.Services.Integrations.Strava;

public sealed class StravaOptions
{
    public const string SectionName = "Integrations:Strava";

    public int ClientId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public int InitialSyncDays { get; set; } = 90;
    public int MaxActivitiesPerSync { get; set; } = 1000;

    public bool IsConfigured => ClientId > 0 && !string.IsNullOrWhiteSpace(ClientSecret);

    public bool HasValidCredentialShape()
    {
        var configuredValues = (ClientId > 0 ? 1 : 0)
            + (!string.IsNullOrWhiteSpace(ClientSecret) ? 1 : 0);
        var callbackIsValid = string.IsNullOrWhiteSpace(CallbackUrl)
            || Uri.TryCreate(CallbackUrl, UriKind.Absolute, out var callback)
                && (callback.Scheme == Uri.UriSchemeHttps
                    || callback.Scheme == Uri.UriSchemeHttp && callback.IsLoopback);
        return configuredValues is 0 or 2
            && callbackIsValid
            && InitialSyncDays is >= 1 and <= 3650
            && MaxActivitiesPerSync is >= 1 and <= 5000;
    }
}
