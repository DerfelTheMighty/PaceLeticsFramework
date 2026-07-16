using PaceLetics.Web.Services.Integrations.Strava;

namespace PaceLetics.Tests;

public sealed class StravaOptionsTests
{
    [Theory]
    [InlineData("https://paceletics.example/integrations/strava/callback", true)]
    [InlineData("http://localhost:5000/integrations/strava/callback", true)]
    [InlineData("http://paceletics.example/integrations/strava/callback", false)]
    [InlineData("javascript:alert(1)", false)]
    public void CallbackRequiresHttpsExceptForLoopback(string callbackUrl, bool expected)
    {
        var options = new StravaOptions
        {
            ClientId = 123,
            ClientSecret = "secret",
            CallbackUrl = callbackUrl
        };

        Assert.Equal(expected, options.HasValidCredentialShape());
    }
}
