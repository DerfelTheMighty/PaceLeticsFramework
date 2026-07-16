using Microsoft.AspNetCore.DataProtection;
using PaceLetics.Web.Services.Integrations.Strava;

namespace PaceLetics.Tests;

public sealed class StravaOAuthStateServiceTests
{
    [Fact]
    public void StateRoundTripPreservesUserAndLocalReturnUrl()
    {
        var service = new StravaOAuthStateService(new EphemeralDataProtectionProvider());

        var state = service.Read(service.Create("user-1", "/Athletes/integrations?source=test"));

        Assert.Equal("user-1", state.AthleteUserId);
        Assert.Equal("/Athletes/integrations?source=test", state.ReturnUrl);
        Assert.False(string.IsNullOrWhiteSpace(state.Nonce));
    }

    [Theory]
    [InlineData("https://attacker.example/path")]
    [InlineData("//attacker.example/path")]
    [InlineData("")]
    public void ExternalOrEmptyReturnUrlFallsBackToIntegrationPage(string returnUrl)
    {
        Assert.Equal("/Athletes/integrations", StravaOAuthStateService.NormalizeReturnUrl(returnUrl));
    }

    [Fact]
    public void TamperedStateIsRejected()
    {
        var service = new StravaOAuthStateService(new EphemeralDataProtectionProvider());
        var protectedState = service.Create("user-1", null);
        var replacement = protectedState[^1] == 'A' ? 'B' : 'A';

        Assert.ThrowsAny<Exception>(() => service.Read(protectedState[..^1] + replacement));
    }
}
