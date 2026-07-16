using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace PaceLetics.Web.Services.Integrations.Strava;

public interface IStravaApiClient
{
    Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StravaApiActivity>> GetActivitiesAsync(
        string accessToken,
        DateTime afterUtc,
        int page,
        int perPage,
        CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
}

public sealed class StravaApiClient : IStravaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly StravaOptions _options;

    public StravaApiClient(HttpClient httpClient, IOptions<StravaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return PostTokenAsync(
            new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId.ToString(CultureInfo.InvariantCulture),
                ["client_secret"] = _options.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code"
            },
            cancellationToken);
    }

    public Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return PostTokenAsync(
            new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId.ToString(CultureInfo.InvariantCulture),
                ["client_secret"] = _options.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            },
            cancellationToken);
    }

    public async Task<IReadOnlyList<StravaApiActivity>> GetActivitiesAsync(
        string accessToken,
        DateTime afterUtc,
        int page,
        int perPage,
        CancellationToken cancellationToken = default)
    {
        var after = new DateTimeOffset(DateTime.SpecifyKind(afterUtc, DateTimeKind.Utc)).ToUnixTimeSeconds();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/v3/athlete/activities?after={after}&page={page}&per_page={perPage}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<StravaApiActivity>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "oauth/revoke")
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["token"] = token,
                    ["token_type_hint"] = "refresh_token"
                })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);
    }

    private async Task<StravaTokenResponse> PostTokenAsync(
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync("oauth/token", new FormUrlEncodedContent(values), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await CreateExceptionAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<StravaTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new StravaApiException(HttpStatusCode.BadGateway, "Strava returned an empty token response.");
    }

    private static async Task<StravaApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (responseText.Length > 2000)
            responseText = responseText[..2000];
        return new StravaApiException(response.StatusCode, responseText);
    }
}

public sealed class StravaApiException : Exception
{
    public StravaApiException(HttpStatusCode statusCode, string responseBody)
        : base($"Strava API request failed with status {(int)statusCode}.")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }
    public string ResponseBody { get; }
}

public sealed class StravaTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("athlete")]
    public StravaApiAthlete? Athlete { get; set; }
}

public sealed class StravaApiAthlete
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = string.Empty;
}

public sealed class StravaApiActivity
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("sport_type")]
    public string SportType { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public DateTime StartDateUtc { get; set; }

    [JsonPropertyName("start_date_local")]
    public DateTime StartDateLocal { get; set; }

    [JsonPropertyName("distance")]
    public double DistanceMeters { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTimeSeconds { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTimeSeconds { get; set; }

    [JsonPropertyName("total_elevation_gain")]
    public double TotalElevationGainMeters { get; set; }

    [JsonPropertyName("average_speed")]
    public double AverageSpeedMetersPerSecond { get; set; }

    [JsonPropertyName("average_heartrate")]
    public double? AverageHeartRate { get; set; }

    [JsonPropertyName("max_heartrate")]
    public double? MaxHeartRate { get; set; }

    [JsonPropertyName("commute")]
    public bool IsCommute { get; set; }

    [JsonPropertyName("trainer")]
    public bool IsTrainer { get; set; }

    [JsonPropertyName("manual")]
    public bool IsManual { get; set; }
}
