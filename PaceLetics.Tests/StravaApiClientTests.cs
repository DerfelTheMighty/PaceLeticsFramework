using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using PaceLetics.Web.Services.Integrations.Strava;

namespace PaceLetics.Tests;

public sealed class StravaApiClientTests
{
    [Fact]
    public async Task ExchangeCodePostsCredentialsAsFormData()
    {
        var handler = new RecordingHandler(_ => Json("""
            {"access_token":"access","refresh_token":"refresh","expires_at":2000000000,"athlete":{"id":42}}
            """));
        var client = CreateClient(handler);

        var response = await client.ExchangeCodeAsync("oauth-code");

        Assert.Equal(42, response.Athlete!.Id);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://www.strava.com/oauth/token", handler.Uri?.ToString());
        Assert.Contains("client_id=123", handler.Body);
        Assert.Contains("client_secret=secret", handler.Body);
        Assert.Contains("code=oauth-code", handler.Body);
        Assert.Contains("grant_type=authorization_code", handler.Body);
    }

    [Fact]
    public async Task GetActivitiesUsesBearerTokenAndIncrementalQuery()
    {
        var handler = new RecordingHandler(_ => Json("""
            [{"id":99,"name":"Lunch Run","type":"Run","sport_type":"Run","start_date":"2026-07-16T10:00:00Z","start_date_local":"2026-07-16T12:00:00","distance":5000}]
            """));
        var client = CreateClient(handler);

        var activities = await client.GetActivitiesAsync(
            "access-token",
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            page: 2,
            perPage: 100);

        Assert.Single(activities);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
        Assert.Equal("access-token", handler.Authorization?.Parameter);
        Assert.Contains("api/v3/athlete/activities?after=", handler.Uri?.ToString());
        Assert.Contains("page=2&per_page=100", handler.Uri?.ToString());
    }

    [Fact]
    public async Task RevokeUsesBasicClientAuthenticationAndRefreshTokenHint()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(handler);

        await client.RevokeAsync("refresh-token");

        var expectedCredentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("123:secret"));
        Assert.Equal("https://www.strava.com/oauth/revoke", handler.Uri?.ToString());
        Assert.Equal("Basic", handler.Authorization?.Scheme);
        Assert.Equal(expectedCredentials, handler.Authorization?.Parameter);
        Assert.Contains("token=refresh-token", handler.Body);
        Assert.Contains("token_type_hint=refresh_token", handler.Body);
    }

    private static StravaApiClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.strava.com/") };
        return new StravaApiClient(
            httpClient,
            Options.Create(new StravaOptions { ClientId = 123, ClientSecret = "secret" }));
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
    };

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? Uri { get; private set; }
        public AuthenticationHeaderValue? Authorization { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            Uri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            Body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }
    }
}
