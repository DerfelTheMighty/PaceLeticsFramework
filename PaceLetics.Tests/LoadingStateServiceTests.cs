using PaceLetics.Web.Services.Loading;

namespace PaceLetics.Tests;

public sealed class LoadingStateServiceTests
{
    [Fact]
    public async Task RunAsync_ActivatesLoadingAndClearsAfterCompletion()
    {
        var loading = new LoadingStateService();
        var observedLoading = false;
        var observedLabel = string.Empty;

        await loading.RunAsync("Loading workouts", () =>
        {
            observedLoading = loading.IsLoading;
            observedLabel = loading.Label;
            return Task.CompletedTask;
        });

        Assert.True(observedLoading);
        Assert.Equal("Loading workouts", observedLabel);
        Assert.False(loading.IsLoading);
        Assert.Equal(string.Empty, loading.Label);
    }

    [Fact]
    public void Show_RestoresPreviousLabelWhenNestedScopeEnds()
    {
        var loading = new LoadingStateService();

        using var outer = loading.Show("Outer");
        using (loading.Show("Inner"))
        {
            Assert.True(loading.IsLoading);
            Assert.Equal("Inner", loading.Label);
        }

        Assert.True(loading.IsLoading);
        Assert.Equal("Outer", loading.Label);
    }
}
