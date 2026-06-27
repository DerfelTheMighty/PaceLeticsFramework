using PaceLetics.Web.Services.AcademyInfo;

namespace PaceLetics.Tests;

public sealed class AcademyInfoServiceTests
{
    [Fact]
    public async Task TrackNavigationAsync_ShowsTipOnCadence()
    {
        var service = CreateService(navigationCadence: 3);
        var changes = 0;
        service.Changed += () => changes++;

        await service.TrackNavigationAsync("https://localhost/Athletes/dashboard");
        await service.TrackNavigationAsync("https://localhost/Athletes/academy");

        Assert.False(service.IsVisible);
        Assert.Null(service.CurrentTip);

        var displayTask = service.TrackNavigationAsync("https://localhost/Athletes/racepaces");

        Assert.True(service.IsVisible);
        Assert.NotNull(service.CurrentTip);
        Assert.True(changes > 0);

        await displayTask;

        Assert.False(service.IsVisible);
    }

    [Fact]
    public async Task TrackNavigationAsync_SkipsIdentityRoutes()
    {
        var service = CreateService(navigationCadence: 1);

        await service.TrackNavigationAsync("https://localhost/Identity/Account/Login");

        Assert.False(service.IsVisible);
        Assert.Null(service.CurrentTip);
    }

    [Fact]
    public void Tips_ContainSmallRepertoire()
    {
        var service = CreateService();

        Assert.True(service.Tips.Count >= 10);
        Assert.All(service.Tips, tip =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tip.Source));
            Assert.False(string.IsNullOrWhiteSpace(tip.Text));
            Assert.StartsWith("/Athletes/academy/", tip.ArticleHref);
        });
    }

    private static AcademyInfoService CreateService(int navigationCadence = 3)
    {
        return new AcademyInfoService(
            TimeProvider.System,
            new Random(7),
            TimeSpan.FromMilliseconds(1),
            navigationCadence,
            TimeSpan.Zero);
    }
}
