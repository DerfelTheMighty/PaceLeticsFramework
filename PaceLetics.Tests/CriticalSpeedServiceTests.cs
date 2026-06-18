using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.CoreModule.Infrastructure.Services;

namespace PaceLetics.Tests;

public class CriticalSpeedServiceTests
{
    private readonly CriticalSpeedService _service = new();

    [Fact]
    public void Estimate_Uses1200SpeedAsRepetitionAnchor()
    {
        var result = CreateResult(RaceKeys.D1200m, RaceDistances.D1200Meters, TimeSpan.FromMinutes(5));

        var model = _service.Estimate([result]);

        Assert.Equal(CriticalSpeedMethod.Single1200Estimate, model.Method);
        Assert.True(model.Estimated);
        Assert.Equal(3.36, model.CriticalSpeedMps, precision: 3);
        Assert.Equal(192, model.DPrimeMeters!.Value, precision: 3);
        Assert.Equal(4.0, model.RepetitionSpeedMps!.Value, precision: 3);
    }

    [Fact]
    public void Estimate_Uses3kSpeedAsIntervalAnchor()
    {
        var result = CreateResult(RaceKeys.D3k, RaceDistances.D3000Meters, TimeSpan.FromMinutes(10.5));

        var model = _service.Estimate([result]);
        var paceModel = _service.BuildPaceModel(model);

        Assert.Equal(CriticalSpeedMethod.SinglePerformanceEstimate, model.Method);
        Assert.True(model.Estimated);
        Assert.Equal(4.286, model.CriticalSpeedMps, precision: 3);
        Assert.Equal(4.69, model.IntervalSpeedMps!.Value, precision: 3);
        Assert.Equal(TimeSpan.FromSeconds(213), paceModel.Intervall);
        Assert.Equal(TimeSpan.FromSeconds(199), paceModel.Repetition);
    }

    [Fact]
    public void Estimate_UsesTwoPointFormulaFor1200And3600()
    {
        var result1200 = CreateResult(RaceKeys.D1200m, RaceDistances.D1200Meters, TimeSpan.FromMinutes(5));
        var result3600 = CreateResult(RaceKeys.D3600m, RaceDistances.D3600Meters, TimeSpan.FromMinutes(16));

        var model = _service.Estimate([result1200, result3600]);

        Assert.Equal(CriticalSpeedMethod.TwoPoint1200And3600, model.Method);
        Assert.False(model.Estimated);
        Assert.Equal(3.636, model.CriticalSpeedMps, precision: 3);
        Assert.Equal(109.091, model.DPrimeMeters!.Value, precision: 3);
        Assert.Equal(4.0, model.RepetitionSpeedMps!.Value, precision: 3);
    }

    [Fact]
    public void BuildPaceModel_MapsCriticalSpeedToExpectedTrainingPaces()
    {
        var model = new CriticalSpeedModel
        {
            CriticalSpeedMps = 4.0,
            IntervalSpeedMps = 4.69,
            RepetitionSpeedMps = 5.05,
            Method = CriticalSpeedMethod.Single1200Estimate
        };

        var paceModel = _service.BuildPaceModel(model);

        Assert.Equal(TimeSpan.FromSeconds(287), paceModel.Easy);
        Assert.Equal(TimeSpan.FromSeconds(266), paceModel.Marathon);
        Assert.Equal(TimeSpan.FromSeconds(245), paceModel.Threshold);
        Assert.Equal(TimeSpan.FromSeconds(213), paceModel.Intervall);
        Assert.Equal(TimeSpan.FromSeconds(198), paceModel.Repetition);
    }

    [Fact]
    public void BuildIntervalRecommendations_UsesDistanceSpecificDPrimeBudgets()
    {
        var model = new CriticalSpeedModel
        {
            CriticalSpeedMps = 4.0,
            DPrimeMeters = 200,
            IntervalSpeedMps = 4.4,
            RepetitionSpeedMps = 4.8
        };

        var recommendations = _service.BuildIntervalRecommendations(model);

        Assert.Equal([200, 400, 800, 1000, 1200, 1600], recommendations.Select(item => item.DistanceMeters));
        Assert.All(recommendations.Where(item => item.DistanceMeters <= 400), item => Assert.True(item.IsFastInterval));
        Assert.All(recommendations.Where(item => item.DistanceMeters >= 800), item => Assert.False(item.IsFastInterval));

        var interval1200 = recommendations.Single(item => item.DistanceMeters == 1200);
        Assert.Equal(TimeSpan.FromSeconds(284), interval1200.WorkTime);
        Assert.Equal(TimeSpan.FromSeconds(142), interval1200.RecoveryTime);
        Assert.Equal(64, interval1200.DPrimeUseMeters, precision: 3);
        Assert.Equal(0.32, interval1200.DPrimeUsePercent, precision: 2);
    }

    [Fact]
    public void BuildIntervalRecommendations_UsesLongRecoveryForFastIntervals()
    {
        var model = new CriticalSpeedModel
        {
            CriticalSpeedMps = 4.0,
            DPrimeMeters = 200,
            IntervalSpeedMps = 4.4,
            RepetitionSpeedMps = 4.8
        };

        var recommendations = _service.BuildIntervalRecommendations(model);

        var fast400 = recommendations.Single(item => item.DistanceMeters == 400);
        Assert.Equal(TimeSpan.FromSeconds(84), fast400.WorkTime);
        Assert.Equal(TimeSpan.FromSeconds(252), fast400.RecoveryTime);
        Assert.Equal(64, fast400.DPrimeUseMeters, precision: 3);
        Assert.True(fast400.TargetSpeedMps <= model.RepetitionSpeedMps);
    }

    private static RaceResultModel CreateResult(string type, long distance, TimeSpan time)
    {
        return new RaceResultModel
        {
            Id = type,
            Type = type,
            DistanceM = distance,
            Time = time,
            Date = DateTime.Today
        };
    }
}
