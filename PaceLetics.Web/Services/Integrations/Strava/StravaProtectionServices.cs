using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace PaceLetics.Web.Services.Integrations.Strava;

public sealed class StravaTokenProtector
{
    private readonly IDataProtector _protector;

    public StravaTokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("PaceLetics.Strava.Tokens.v1");
    }

    public string Protect(string token) => _protector.Protect(token);
    public string Unprotect(string protectedToken) => _protector.Unprotect(protectedToken);
}

public sealed class StravaOAuthStateService
{
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);
    private readonly ITimeLimitedDataProtector _protector;

    public StravaOAuthStateService(IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector("PaceLetics.Strava.OAuthState.v1")
            .ToTimeLimitedDataProtector();
    }

    public string Create(string athleteUserId, string? returnUrl)
    {
        var state = new StravaOAuthState(
            athleteUserId,
            NormalizeReturnUrl(returnUrl),
            Guid.NewGuid().ToString("N"));
        return _protector.Protect(JsonSerializer.Serialize(state), StateLifetime);
    }

    public StravaOAuthState Read(string protectedState)
    {
        var json = _protector.Unprotect(protectedState);
        return JsonSerializer.Deserialize<StravaOAuthState>(json)
            ?? throw new InvalidOperationException("The Strava OAuth state is invalid.");
    }

    public static string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)
            || !returnUrl.StartsWith("/", StringComparison.Ordinal)
            || returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return "/Athletes/integrations";
        }

        return returnUrl;
    }
}

public sealed record StravaOAuthState(string AthleteUserId, string ReturnUrl, string Nonce);
