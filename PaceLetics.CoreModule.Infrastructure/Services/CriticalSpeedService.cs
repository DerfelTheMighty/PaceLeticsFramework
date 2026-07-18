using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Services
{
    public class CriticalSpeedService : ICriticalSpeedService
    {
        private const double Single1200CriticalSpeedFactor = 0.84;
        private const double Single3000CriticalSpeedFactor = 0.90;
        private const double IntervalFallbackFactor = 1.10;
        private const double RepetitionFallbackFactorOverInterval = 1.07;
        private const double IntervalFrom3000AnchorFactor = 0.985;
        private const double IntervalFrom1200AnchorFactor = 1 / RepetitionFallbackFactorOverInterval;
        private const double LongIntervalFallbackSpeedFactor = 1.10;
        private const double FastIntervalFallbackSpeedFactor = 1.18;

        public CriticalSpeedModel Estimate(IEnumerable<RaceResultModel> results)
        {
            var validResults = GetLatestValidResults(results);

            if (validResults.Count == 0)
                return new CriticalSpeedModel();

            var result1200 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D1200Meters);
            var result3000 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D3000Meters);
            var result3600 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D3600Meters);

            if (result1200 is not null && result3600 is not null)
                return EstimateFromTwoPoint1200And3600(result1200, result3600, result3000);

            if (validResults.Count >= 2)
                return EstimateFromRegression(validResults);

            return EstimateFromSinglePerformance(validResults[0]);
        }

        public IReadOnlyList<RaceResultModel> GetContributingResults(IEnumerable<RaceResultModel> results)
        {
            var validResults = GetLatestValidResults(results);
            var result1200 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D1200Meters);
            var result3600 = validResults.FirstOrDefault(result => result.DistanceM == RaceDistances.D3600Meters);

            return result1200 is not null && result3600 is not null
                ? [result1200, result3600]
                : validResults;
        }

        public PaceModel BuildPaceModel(CriticalSpeedModel model)
        {
            if (!model.IsValid)
                return new PaceModel();

            var intervalSpeed = EstimateIntervalSpeed(model);
            var fastIntervalSpeed = EstimateFastIntervalSpeed(model, intervalSpeed);

            return new PaceModel
            {
                CriticalSpeedMps = model.CriticalSpeedMps,
                Recovery = PaceFromSpeed(model.CriticalSpeedMps * 0.76),
                Easy = PaceFromSpeed(model.CriticalSpeedMps * 0.87),
                Threshold = PaceFromSpeed(model.CriticalSpeedMps * 0.98),
                Intervall = PaceFromSpeed(intervalSpeed),
                FastIntervall = PaceFromSpeed(fastIntervalSpeed)
            };
        }

        public IReadOnlyList<CriticalSpeedIntervalRecommendation> BuildIntervalRecommendations(CriticalSpeedModel model)
        {
            if (!model.IsValid || model.DPrimeMeters is not > 0)
                return [];

            var longIntervalSpeedCap = model.IntervalSpeedMps is > 0
                ? model.IntervalSpeedMps.Value
                : model.CriticalSpeedMps * LongIntervalFallbackSpeedFactor;
            var fastIntervalSpeedCap = model.RepetitionSpeedMps is > 0
                ? model.RepetitionSpeedMps.Value
                : model.CriticalSpeedMps * FastIntervalFallbackSpeedFactor;

            return
            [
                BuildRecommendation("fast-200", "fast-intervall", "Fast", 200, 0.08, true, fastIntervalSpeedCap, 3, model),
                BuildRecommendation("fast-400", "fast-intervall", "Fast", 400, 0.12, true, fastIntervalSpeedCap, 3, model),
                BuildRecommendation("interval-800", "intervall", "Int", 800, 0.22, false, longIntervalSpeedCap, 0.5, model),
                BuildRecommendation("interval-1000", "intervall", "Int", 1000, 0.27, false, longIntervalSpeedCap, 0.5, model),
                BuildRecommendation("interval-1200", "intervall", "Int", 1200, 0.32, false, longIntervalSpeedCap, 0.5, model),
                BuildRecommendation("interval-1600", "intervall", "Int", 1600, 0.42, false, longIntervalSpeedCap, 0.5, model)
            ];
        }

        private static CriticalSpeedModel EstimateFromTwoPoint1200And3600(
            RaceResultModel result1200,
            RaceResultModel result3600,
            RaceResultModel? result3000)
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
                IntervalSpeedMps = result3000 is null ? null : Speed(result3000) * IntervalFrom3000AnchorFactor,
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
            var result3000 = results.FirstOrDefault(result => result.DistanceM == RaceDistances.D3000Meters);

            return new CriticalSpeedModel
            {
                CriticalSpeedMps = criticalSpeed,
                DPrimeMeters = Math.Max(0, dPrime),
                IntervalSpeedMps = result3000 is null ? null : Speed(result3000) * IntervalFrom3000AnchorFactor,
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
            var is3000 = result.DistanceM == RaceDistances.D3000Meters;
            var criticalSpeedFactor = result.DistanceM switch
            {
                RaceDistances.D1200Meters => Single1200CriticalSpeedFactor,
                RaceDistances.D3000Meters => Single3000CriticalSpeedFactor,
                _ => GetSinglePerformanceFactor(result.Time)
            };

            return new CriticalSpeedModel
            {
                CriticalSpeedMps = speed * criticalSpeedFactor,
                DPrimeMeters = result.DistanceM * (1 - criticalSpeedFactor),
                IntervalSpeedMps = is3000 ? speed * IntervalFrom3000AnchorFactor : null,
                RepetitionSpeedMps = is1200 ? speed : null,
                Estimated = true,
                Method = is1200 ? CriticalSpeedMethod.Single1200Estimate : CriticalSpeedMethod.SinglePerformanceEstimate,
                SourceResultCount = 1
            };
        }

        private static double EstimateIntervalSpeed(CriticalSpeedModel model)
        {
            if (model.IntervalSpeedMps is > 0)
                return model.IntervalSpeedMps.Value;

            if (model.RepetitionSpeedMps is > 0)
                return model.RepetitionSpeedMps.Value * IntervalFrom1200AnchorFactor;

            return model.CriticalSpeedMps * IntervalFallbackFactor;
        }

        private static double EstimateFastIntervalSpeed(CriticalSpeedModel model, double intervalSpeed)
        {
            if (model.RepetitionSpeedMps is > 0)
                return model.RepetitionSpeedMps.Value;

            return intervalSpeed * RepetitionFallbackFactorOverInterval;
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

        private static List<RaceResultModel> GetLatestValidResults(IEnumerable<RaceResultModel> results)
        {
            ArgumentNullException.ThrowIfNull(results);

            return results
                .Where(IsValid)
                .GroupBy(result => result.DistanceM)
                .Select(group => group.OrderByDescending(result => result.Date).First())
                .OrderBy(result => result.DistanceM)
                .ToList();
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

        private static CriticalSpeedIntervalRecommendation BuildRecommendation(
            string key,
            string zoneKey,
            string code,
            int distanceMeters,
            double dPrimeBudgetPercent,
            bool isFastInterval,
            double speedCap,
            double recoveryMultiplier,
            CriticalSpeedModel model)
        {
            var targetDPrimeMeters = model.DPrimeMeters!.Value * dPrimeBudgetPercent;
            var availableDistanceAtCriticalSpeed = Math.Max(1, distanceMeters - targetDPrimeMeters);
            var budgetSpeed = distanceMeters * model.CriticalSpeedMps / availableDistanceAtCriticalSpeed;
            var targetSpeed = Math.Min(budgetSpeed, speedCap);
            var workTime = TimeSpan.FromSeconds(Math.Round(distanceMeters / targetSpeed));
            var dPrimeUseMeters = Math.Max(0, distanceMeters - model.CriticalSpeedMps * workTime.TotalSeconds);
            var dPrimeUsePercent = dPrimeUseMeters / model.DPrimeMeters.Value;

            return new CriticalSpeedIntervalRecommendation
            {
                Key = key,
                ZoneKey = zoneKey,
                Code = code,
                DistanceMeters = distanceMeters,
                TargetSpeedMps = targetSpeed,
                TargetPace = PaceFromSpeed(targetSpeed),
                WorkTime = workTime,
                RecoveryTime = TimeSpan.FromSeconds(Math.Round(workTime.TotalSeconds * recoveryMultiplier)),
                DPrimeUseMeters = dPrimeUseMeters,
                DPrimeUsePercent = dPrimeUsePercent,
                TargetDPrimeBudgetPercent = dPrimeBudgetPercent,
                IsFastInterval = isFastInterval
            };
        }
    }
}
