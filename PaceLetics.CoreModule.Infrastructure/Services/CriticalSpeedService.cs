using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Services
{
    public class CriticalSpeedService : ICriticalSpeedService
    {
        private const double Single1200CriticalSpeedFactor = 0.90;
        private const double RepetitionSpeedFallbackFactor = 1.11;

        public CriticalSpeedModel Estimate(IEnumerable<RaceResultModel> results)
        {
            var validResults = results
                .Where(IsValid)
                .GroupBy(result => result.DistanceM)
                .Select(group => group.OrderByDescending(result => result.Date).First())
                .OrderBy(result => result.DistanceM)
                .ToList();

            if (validResults.Count == 0)
                return new CriticalSpeedModel();

            var result1200 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D1200Meters);
            var result3600 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D3600Meters);

            if (result1200 is not null && result3600 is not null)
                return EstimateFromTwoPoint1200And3600(result1200, result3600);

            if (validResults.Count >= 2)
                return EstimateFromRegression(validResults);

            return EstimateFromSinglePerformance(validResults[0]);
        }

        public PaceModel BuildPaceModel(CriticalSpeedModel model)
        {
            if (!model.IsValid)
                return new PaceModel();

            return new PaceModel
            {
                Vdot = model.CriticalSpeedMps,
                Easy = PaceFromSpeed(model.CriticalSpeedMps * 0.87),
                Marathon = PaceFromSpeed(model.CriticalSpeedMps * 0.94),
                Threshold = PaceFromSpeed(model.CriticalSpeedMps * 1.02),
                Intervall = PaceFromSpeed(model.CriticalSpeedMps * 1.09),
                Repetition = PaceFromSpeed(model.RepetitionSpeedMps ?? model.CriticalSpeedMps * RepetitionSpeedFallbackFactor)
            };
        }

        private static CriticalSpeedModel EstimateFromTwoPoint1200And3600(RaceResultModel result1200, RaceResultModel result3600)
        {
            var time1200 = result1200.Time.TotalSeconds;
            var time3600 = result3600.Time.TotalSeconds;

            if (time3600 <= time1200)
                return EstimateFromRegression([result1200, result3600]);

            var criticalSpeed = (result3600.DistanceM - result1200.DistanceM) / (time3600 - time1200);
            var dPrime = result1200.DistanceM - criticalSpeed * time1200;

            return new CriticalSpeedModel
            {
                CriticalSpeedMps = criticalSpeed,
                DPrimeMeters = Math.Max(0, dPrime),
                RepetitionSpeedMps = Speed(result1200),
                Estimated = false,
                Method = CriticalSpeedMethod.TwoPoint1200And3600,
                SourceResultCount = 2
            };
        }

        private static CriticalSpeedModel EstimateFromRegression(IReadOnlyCollection<RaceResultModel> results)
        {
            var count = results.Count;
            var sumTime = results.Sum(result => result.Time.TotalSeconds);
            var sumDistance = results.Sum(result => (double)result.DistanceM);
            var sumTimeDistance = results.Sum(result => result.Time.TotalSeconds * result.DistanceM);
            var sumTimeSquared = results.Sum(result => result.Time.TotalSeconds * result.Time.TotalSeconds);
            var denominator = count * sumTimeSquared - sumTime * sumTime;

            if (Math.Abs(denominator) < double.Epsilon)
                return EstimateFromSinglePerformance(results.OrderBy(result => result.DistanceM).First());

            var criticalSpeed = (count * sumTimeDistance - sumTime * sumDistance) / denominator;
            var dPrime = (sumDistance - criticalSpeed * sumTime) / count;
            var result1200 = results.FirstOrDefault(result => result.DistanceM == RaceDistances.D1200Meters);

            return new CriticalSpeedModel
            {
                CriticalSpeedMps = criticalSpeed,
                DPrimeMeters = Math.Max(0, dPrime),
                RepetitionSpeedMps = result1200 is null ? null : Speed(result1200),
                Estimated = false,
                Method = CriticalSpeedMethod.MultiPointRegression,
                SourceResultCount = count
            };
        }

        private static CriticalSpeedModel EstimateFromSinglePerformance(RaceResultModel result)
        {
            var speed = Speed(result);
            var is1200 = result.DistanceM == RaceDistances.D1200Meters;

            return new CriticalSpeedModel
            {
                CriticalSpeedMps = speed * (is1200 ? Single1200CriticalSpeedFactor : GetSinglePerformanceFactor(result.Time)),
                RepetitionSpeedMps = is1200 ? speed : null,
                Estimated = true,
                Method = is1200 ? CriticalSpeedMethod.Single1200Estimate : CriticalSpeedMethod.SinglePerformanceEstimate,
                SourceResultCount = 1
            };
        }

        private static double GetSinglePerformanceFactor(TimeSpan time)
        {
            var minutes = time.TotalMinutes;

            if (minutes <= 4)
                return 0.90;
            if (minutes <= 8)
                return 0.93;
            if (minutes <= 15)
                return 0.97;
            if (minutes <= 35)
                return 1.00;
            if (minutes <= 75)
                return 1.03;

            return 1.07;
        }

        private static bool IsValid(RaceResultModel result)
        {
            return result.DistanceM > 0 && result.Time.TotalSeconds > 0;
        }

        private static double Speed(RaceResultModel result)
        {
            return result.DistanceM / result.Time.TotalSeconds;
        }

        private static TimeSpan PaceFromSpeed(double metersPerSecond)
        {
            return metersPerSecond <= 0
                ? default
                : TimeSpan.FromSeconds(Math.Round(1000 / metersPerSecond));
        }
    }
}
