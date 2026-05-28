using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.Tests;

public class PaceModelTests
{
    [Fact]
    public void GetPace_UsesTypedPaceEnum()
    {
        var model = CreatePaceModel();

        var pace = model.GetPace(Pace.Marathon);

        Assert.Equal(TimeSpan.FromMinutes(5), pace);
    }

    [Fact]
    public void GetPace_ThrowsForUnknownStringKey()
    {
        var model = CreatePaceModel();

        Assert.Throws<ArgumentException>(() => model.GetPace("unknown"));
    }

    [Fact]
    public void TryGetPace_ReturnsFalseForFreePace()
    {
        var model = CreatePaceModel();

        var resolved = model.TryGetPace(PaceKeys.Free, out var pace);

        Assert.False(resolved);
        Assert.Equal(default, pace);
    }

    [Fact]
    public void TryGetPace_RecoveryPaceIsThirtySecondsSlowerThanEasyPace()
    {
        var model = CreatePaceModel();

        var resolved = model.TryGetPace(PaceKeys.Recovery, out var pace);

        Assert.True(resolved);
        Assert.Equal(TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(30)), pace);
    }

    internal static PaceModel CreatePaceModel()
    {
        return new PaceModel
        {
            Easy = TimeSpan.FromMinutes(6),
            Marathon = TimeSpan.FromMinutes(5),
            Threshold = TimeSpan.FromMinutes(4),
            Intervall = TimeSpan.FromMinutes(3),
            Repetition = TimeSpan.FromMinutes(2)
        };
    }
}
