using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.CoreModule.Infrastructure.Models;
using PaceLetics.CoreModule.Infrastructure.Services;

namespace PaceLetics.Tests;

public class CriticalSpeedServiceTests
{
    private readonly CriticalSpeedService _service = new();

    [Fact]
    public void Estimate_UsesNinetyPercentOf1200SpeedForSingle1200Test()
    {
        var result = CreateResult(RaceKeys.D1200m, RaceDistances.D1200Meters, TimeSpan.FromMinutes(5));

        var model = _service.Estimate([result]);

        Assert.Equal(CriticalSpeedMethod.Single1200Estimate, model.Method);
        Assert.True(model.Estimated);
        Assert.Equal(3.6, model.CriticalSpeedMps, precision: 3);
        Assert.Equal(4.0, model.RepetitionSpeedMps!.Value, precision: 3);
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
            RepetitionSpeedMps = 4.5,
            Method = CriticalSpeedMethod.Single1200Estimate
        };

        var paceModel = _service.BuildPaceModel(model);

        Assert.Equal(TimeSpan.FromSeconds(287), paceModel.Easy);
        Assert.Equal(TimeSpan.FromSeconds(266), paceModel.Marathon);
        Assert.Equal(TimeSpan.FromSeconds(245), paceModel.Threshold);
        Assert.Equal(TimeSpan.FromSeconds(229), paceModel.Intervall);
        Assert.Equal(TimeSpan.FromSeconds(222), paceModel.Repetition);
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
