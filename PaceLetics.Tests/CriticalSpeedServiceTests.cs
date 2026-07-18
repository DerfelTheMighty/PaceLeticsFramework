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
        Assert.Equal(TimeSpan.FromSeconds(199), paceModel.FastIntervall);
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
        Assert.Equal(250, model.PredictPaceSecondsPerKilometer(1200)!.Value, precision: 3);
        Assert.Equal(266.667, model.PredictPaceSecondsPerKilometer(3600)!.Value, precision: 3);
    }

    [Fact]
    public void GetContributingResults_UsesOnly1200And3600WhenBothAreAvailable()
    {
        var result1200 = CreateResult(RaceKeys.D1200m, RaceDistances.D1200Meters, TimeSpan.FromMinutes(5));
        var result3000 = CreateResult(RaceKeys.D3k, RaceDistances.D3000Meters, TimeSpan.FromMinutes(11));
        var result3600 = CreateResult(RaceKeys.D3600m, RaceDistances.D3600Meters, TimeSpan.FromMinutes(16));

        var contributors = _service.GetContributingResults([result1200, result3000, result3600]);

        Assert.Equal([result1200, result3600], contributors);
    }

    [Fact]
    public void GetContributingResults_UsesAllLatestDistancesForRegression()
    {
        var old3000 = CreateResult("old-3k", RaceDistances.D3000Meters, TimeSpan.FromMinutes(12));
        old3000.Date = DateTime.Today.AddDays(-7);
        var latest3000 = CreateResult("latest-3k", RaceDistances.D3000Meters, TimeSpan.FromMinutes(11));
        var result5000 = CreateResult(RaceKeys.D5k, 5000, TimeSpan.FromMinutes(19));

        var contributors = _service.GetContributingResults([old3000, latest3000, result5000]);

        Assert.Equal([latest3000, result5000], contributors);
    }

    [Fact]
    public void BuildEnduranceProjection_ReachesCriticalPaceBeforeLongDistanceDecay()
    {
        var result1200 = CreateResult(RaceKeys.D1200m, RaceDistances.D1200Meters, TimeSpan.FromMinutes(5));
        var result3600 = CreateResult(RaceKeys.D3600m, RaceDistances.D3600Meters, TimeSpan.FromMinutes(16));
        var criticalSpeed = _service.Estimate([result1200, result3600]);

        var projection = _service.BuildEnduranceProjection(
            criticalSpeed,
            RaceDistances.D3600Meters);
        var criticalDistancePace = projection.PredictPaceSecondsPerKilometer(
            projection.CriticalDistanceMeters);
        var marathonPace = projection.PredictPaceSecondsPerKilometer(RaceDistances.DMarathonMeters);
        var criticalPace = 1000 / criticalSpeed.CriticalSpeedMps;

        Assert.True(projection.IsValid);
        Assert.InRange(projection.CriticalDistanceMeters, 5000, 10000);
        Assert.Equal(criticalPace, criticalDistancePace!.Value, precision: 3);
        Assert.NotNull(marathonPace);
        Assert.Equal(criticalPace / 0.85, marathonPace!.Value, precision: 3);
    }

    [Fact]
    public void BuildEnduranceProjection_MapsThreeFortyFiveCsToFourTwentyFiveMarathonPace()
    {
        var criticalSpeed = new CriticalSpeedModel
        {
            CriticalSpeedMps = 1000.0 / 225,
            DPrimeMeters = 150
        };

        var projection = _service.BuildEnduranceProjection(
            criticalSpeed,
            RaceDistances.D3600Meters);
        var fiveKilometerPace = projection.PredictPaceSecondsPerKilometer(5000);
        var tenKilometerPace = projection.PredictPaceSecondsPerKilometer(10000);
        var marathonPace = projection.PredictPaceSecondsPerKilometer(RaceDistances.DMarathonMeters);
        var marathonRange = projection.PredictPaceRange(RaceDistances.DMarathonMeters);

        Assert.True(projection.IsValid);
        Assert.Equal(8000, projection.CriticalDistanceMeters, precision: 3);
        Assert.True(fiveKilometerPace!.Value < 225);
        Assert.True(tenKilometerPace!.Value > 225);
        Assert.Equal(264.706, marathonPace!.Value, precision: 3);
        Assert.Equal(0.01, projection.PredictSpeedFractionError(RaceDistances.D3600Meters)!.Value, precision: 3);
        Assert.Equal(0.02, projection.PredictSpeedFractionError(8000)!.Value, precision: 3);
        Assert.Equal(0.10, projection.PredictSpeedFractionError(RaceDistances.DMarathonMeters)!.Value, precision: 3);
        Assert.Equal(236.842, marathonRange!.Value.FasterPaceSecondsPerKilometer, precision: 3);
        Assert.Equal(300, marathonRange.Value.SlowerPaceSecondsPerKilometer, precision: 3);
    }

    [Fact]
    public void BuildEnduranceProjection_IsMonotoneWithoutOvershoot()
    {
        var criticalSpeed = new CriticalSpeedModel
        {
            CriticalSpeedMps = 1000.0 / 225,
            DPrimeMeters = 150
        };
        var projection = _service.BuildEnduranceProjection(
            criticalSpeed,
            RaceDistances.D3600Meters);
        var distances = Enumerable.Range(0, 201)
            .Select(index => projection.AnchorDistanceMeters
                * Math.Pow(
                    projection.MarathonDistanceMeters / projection.AnchorDistanceMeters,
                    index / 200.0))
            .ToList();
        var paces = distances
            .Select(distance => projection.PredictPaceSecondsPerKilometer(distance)!.Value)
            .ToList();
        var errors = distances
            .Select(distance => projection.PredictSpeedFractionError(distance)!.Value)
            .ToList();

        Assert.Equal(
            criticalSpeed.PredictPaceSecondsPerKilometer(RaceDistances.D3600Meters)!.Value,
            paces[0],
            precision: 6);
        Assert.All(paces.Zip(paces.Skip(1)), pair => Assert.True(pair.Second >= pair.First));
        Assert.All(errors.Zip(errors.Skip(1)), pair => Assert.True(pair.Second >= pair.First));
        Assert.Equal(225 / 0.85, paces[^1], precision: 6);
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

        Assert.Equal(4.0, paceModel.CriticalSpeedMps);
        Assert.Equal(TimeSpan.FromSeconds(329), paceModel.Recovery);
        Assert.Equal(TimeSpan.FromSeconds(287), paceModel.Easy);
        Assert.Equal(TimeSpan.FromSeconds(255), paceModel.Threshold);
        Assert.Equal(TimeSpan.FromSeconds(213), paceModel.Intervall);
        Assert.Equal(TimeSpan.FromSeconds(198), paceModel.FastIntervall);
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

        var fast200 = recommendations.Single(item => item.DistanceMeters == 200);
        Assert.Equal(TimeSpan.FromSeconds(46), fast200.WorkTime);
        Assert.Equal(TimeSpan.FromSeconds(138), fast200.RecoveryTime);
        Assert.Equal(16, fast200.DPrimeUseMeters, precision: 3);
        Assert.Equal(0.08, fast200.DPrimeUsePercent, precision: 2);

        var fast400 = recommendations.Single(item => item.DistanceMeters == 400);
        Assert.Equal(TimeSpan.FromSeconds(94), fast400.WorkTime);
        Assert.Equal(TimeSpan.FromSeconds(282), fast400.RecoveryTime);
        Assert.Equal(24, fast400.DPrimeUseMeters, precision: 3);
        Assert.Equal(0.12, fast400.DPrimeUsePercent, precision: 2);
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
