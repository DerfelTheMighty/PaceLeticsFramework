using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PaceLetics.Web.Services.Integrations.Strava;

namespace PaceLetics.Tests;

public sealed class StravaIntegrationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 16, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CompleteAuthorizationStoresProtectedRotatingTokens()
    {
        var repository = new FakeRepository();
        var api = new FakeApi
        {
            ExchangeResponse = TokenResponse("access-1", "refresh-1", Now.AddHours(6), "read,activity:read_all")
        };
        var protector = CreateProtector();
        var service = CreateService(repository, api, protector);

        await service.CompleteAuthorizationAsync("user-1", "code", "");

        Assert.NotNull(repository.Connection);
        Assert.NotEqual("access-1", repository.Connection.ProtectedAccessToken);
        Assert.Equal("access-1", protector.Unprotect(repository.Connection.ProtectedAccessToken));
        Assert.Equal("refresh-1", protector.Unprotect(repository.Connection.ProtectedRefreshToken));
        Assert.Equal(1234, repository.Connection.StravaAthleteId);
        Assert.Contains("activity:read_all", repository.Connection.Scopes);
    }

    [Fact]
    public async Task CompleteAuthorizationRevokesTokenWhenActivityScopeIsMissing()
    {
        var repository = new FakeRepository();
        var api = new FakeApi
        {
            ExchangeResponse = TokenResponse("access-1", "refresh-1", Now.AddHours(6), "read")
        };
        var service = CreateService(repository, api, CreateProtector());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CompleteAuthorizationAsync("user-1", "code", "read"));

        Assert.Null(repository.Connection);
        Assert.Equal("refresh-1", api.RevokedToken);
    }

    [Fact]
    public async Task ConnectingAnotherStravaAccountRemovesPreviousAccountData()
    {
        var repository = new FakeRepository();
        var protector = CreateProtector();
        repository.Connection = Connection(protector, Now.AddHours(3));
        repository.Connection.StravaAthleteId = 9999;
        repository.Activities.Add(new StravaActivityDocument { StravaActivityId = 10 });
        var api = new FakeApi
        {
            ExchangeResponse = TokenResponse("access-2", "refresh-2", Now.AddHours(6), "activity:read_all")
        };
        var service = CreateService(repository, api, protector);

        await service.CompleteAuthorizationAsync("user-1", "code", "activity:read_all");

        Assert.Empty(repository.Activities);
        Assert.Equal(1234, repository.Connection!.StravaAthleteId);
        Assert.Equal("refresh-2", protector.Unprotect(repository.Connection.ProtectedRefreshToken));
    }

    [Fact]
    public async Task SyncImportsOnlyRunsAndUsesInitialLookback()
    {
        var repository = new FakeRepository();
        var protector = CreateProtector();
        repository.Connection = Connection(protector, Now.AddHours(3));
        var api = new FakeApi
        {
            Activities =
            [
                Activity(10, "Morning Run", "Run"),
                Activity(11, "Bike Ride", "Ride"),
                Activity(12, "Trail", "TrailRun")
            ]
        };
        var service = CreateService(repository, api, protector);

        var result = await service.SyncAsync("user-1");

        Assert.Equal(2, result.ImportedActivities);
        Assert.Equal(2, repository.Activities.Count);
        Assert.All(repository.Activities, item => Assert.DoesNotContain("Ride", item.Name));
        Assert.Equal(Now.UtcDateTime.AddDays(-90), api.RequestedAfterUtc);
        Assert.Equal(Now.UtcDateTime, repository.Connection.LastSyncAt);
    }

    [Fact]
    public async Task SyncPersistsLatestTokensReturnedByRefresh()
    {
        var repository = new FakeRepository();
        var protector = CreateProtector();
        repository.Connection = Connection(protector, Now.AddMinutes(10));
        var api = new FakeApi
        {
            RefreshResponse = TokenResponse("access-2", "refresh-2", Now.AddHours(6), "read,activity:read_all")
        };
        var service = CreateService(repository, api, protector);

        await service.SyncAsync("user-1");

        Assert.Equal("refresh-1", api.RefreshedToken);
        Assert.Equal("access-2", protector.Unprotect(repository.Connection!.ProtectedAccessToken));
        Assert.Equal("refresh-2", protector.Unprotect(repository.Connection.ProtectedRefreshToken));
    }

    [Fact]
    public async Task DisconnectRevokesRefreshTokenAndDeletesStoredData()
    {
        var repository = new FakeRepository();
        var protector = CreateProtector();
        repository.Connection = Connection(protector, Now.AddHours(3));
        repository.Activities.Add(new StravaActivityDocument { StravaActivityId = 10 });
        var api = new FakeApi();
        var service = CreateService(repository, api, protector);

        await service.DisconnectAsync("user-1");

        Assert.Equal("refresh-1", api.RevokedToken);
        Assert.Null(repository.Connection);
        Assert.Empty(repository.Activities);
    }

    private static StravaIntegrationService CreateService(
        FakeRepository repository,
        FakeApi api,
        StravaTokenProtector protector)
    {
        return new StravaIntegrationService(
            repository,
            api,
            protector,
            Options.Create(new StravaOptions
            {
                ClientId = 123,
                ClientSecret = "secret",
                InitialSyncDays = 90,
                MaxActivitiesPerSync = 1000
            }),
            new FixedTimeProvider(Now),
            NullLogger<StravaIntegrationService>.Instance);
    }

    private static StravaTokenProtector CreateProtector() =>
        new(new EphemeralDataProtectionProvider());

    private static StravaConnectionDocument Connection(StravaTokenProtector protector, DateTimeOffset expiresAt) => new()
    {
        AthleteUserId = "user-1",
        ProtectedAccessToken = protector.Protect("access-1"),
        ProtectedRefreshToken = protector.Protect("refresh-1"),
        AccessTokenExpiresAt = expiresAt.UtcDateTime,
        Scopes = ["activity:read_all"],
        ConnectedAt = Now.UtcDateTime
    };

    private static StravaTokenResponse TokenResponse(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        string scope) => new()
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = expiresAt.ToUnixTimeSeconds(),
        Scope = scope,
        Athlete = new StravaApiAthlete { Id = 1234, FirstName = "Ada", LastName = "Runner" }
    };

    private static StravaApiActivity Activity(long id, string name, string sportType) => new()
    {
        Id = id,
        Name = name,
        Type = sportType == "Ride" ? "Ride" : "Run",
        SportType = sportType,
        StartDateUtc = Now.UtcDateTime.AddDays(-1),
        StartDateLocal = Now.DateTime.AddDays(-1),
        DistanceMeters = 5000,
        MovingTimeSeconds = 1500,
        AverageSpeedMetersPerSecond = 3.33
    };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeRepository : IStravaIntegrationRepository
    {
        public StravaConnectionDocument? Connection { get; set; }
        public List<StravaActivityDocument> Activities { get; } = [];

        public Task<StravaConnectionDocument?> GetConnectionAsync(string athleteUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Connection);

        public Task UpsertConnectionAsync(StravaConnectionDocument connection, CancellationToken cancellationToken = default)
        {
            Connection = connection;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StravaActivityDocument>> GetActivitiesAsync(string athleteUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<StravaActivityDocument>>(Activities.OrderByDescending(item => item.StartDateUtc).ToList());

        public Task UpsertActivitiesAsync(IReadOnlyCollection<StravaActivityDocument> activities, CancellationToken cancellationToken = default)
        {
            foreach (var activity in activities)
            {
                Activities.RemoveAll(existing => existing.StravaActivityId == activity.StravaActivityId);
                Activities.Add(activity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAllAsync(string athleteUserId, CancellationToken cancellationToken = default)
        {
            Connection = null;
            Activities.Clear();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeApi : IStravaApiClient
    {
        public StravaTokenResponse? ExchangeResponse { get; init; }
        public StravaTokenResponse? RefreshResponse { get; init; }
        public IReadOnlyList<StravaApiActivity> Activities { get; init; } = [];
        public string? RefreshedToken { get; private set; }
        public string? RevokedToken { get; private set; }
        public DateTime? RequestedAfterUtc { get; private set; }

        public Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExchangeResponse ?? throw new InvalidOperationException("No exchange response configured."));

        public Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshedToken = refreshToken;
            return Task.FromResult(RefreshResponse ?? throw new InvalidOperationException("No refresh response configured."));
        }

        public Task<IReadOnlyList<StravaApiActivity>> GetActivitiesAsync(
            string accessToken,
            DateTime afterUtc,
            int page,
            int perPage,
            CancellationToken cancellationToken = default)
        {
            RequestedAfterUtc = afterUtc;
            return Task.FromResult(page == 1 ? Activities : (IReadOnlyList<StravaApiActivity>)[]);
        }

        public Task RevokeAsync(string token, CancellationToken cancellationToken = default)
        {
            RevokedToken = token;
            return Task.CompletedTask;
        }
    }
}
