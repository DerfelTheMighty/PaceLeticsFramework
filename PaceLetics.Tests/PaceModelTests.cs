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

        var pace = model.GetPace(Pace.FastIntervall);

        Assert.Equal(TimeSpan.FromMinutes(2), pace);
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
    public void TryGetPace_ReturnsFalseForWalk()
    {
        var model = CreatePaceModel();

        var resolved = model.TryGetPace(PaceKeys.Walk, out var pace);

        Assert.False(resolved);
        Assert.Equal(default, pace);
    }

    [Theory]
    [InlineData("E Pace", PaceKeys.Easy)]
    [InlineData("M Pace", PaceKeys.Easy)]
    [InlineData("T Pace", PaceKeys.Threshold)]
    [InlineData("I Pace", PaceKeys.Intervall)]
    [InlineData("R Pace", PaceKeys.FastIntervall)]
    public void Normalize_MapsLegacyDanielsKeysToCurrentZones(string legacyKey, string expected)
    {
        Assert.Equal(expected, PaceKeys.Normalize(legacyKey));
    }

    [Fact]
    public void TryGetPace_UsesCriticalSpeedRecoveryPace()
    {
        var model = CreatePaceModel();

        var resolved = model.TryGetPace(PaceKeys.Recovery, out var pace);

        Assert.True(resolved);
        Assert.Equal(TimeSpan.FromMinutes(7), pace);
    }

    internal static PaceModel CreatePaceModel()
    {
        return new PaceModel
        {
            Easy = TimeSpan.FromMinutes(6),
            Recovery = TimeSpan.FromMinutes(7),
            Threshold = TimeSpan.FromMinutes(4),
            Intervall = TimeSpan.FromMinutes(3),
            FastIntervall = TimeSpan.FromMinutes(2)
        };
    }
}
