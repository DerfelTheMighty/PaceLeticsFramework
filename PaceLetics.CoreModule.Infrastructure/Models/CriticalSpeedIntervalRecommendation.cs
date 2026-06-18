namespace PaceLetics.CoreModule.Infrastructure.Models
{
    public class CriticalSpeedIntervalRecommendation
    {
        public string Key { get; set; } = string.Empty;

        public string ZoneKey { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public int DistanceMeters { get; set; }

        public double TargetSpeedMps { get; set; }

        public TimeSpan TargetPace { get; set; }

        public TimeSpan WorkTime { get; set; }

        public TimeSpan RecoveryTime { get; set; }

        public double DPrimeUseMeters { get; set; }

        public double DPrimeUsePercent { get; set; }

        public double TargetDPrimeBudgetPercent { get; set; }

        public bool IsFastInterval { get; set; }
    }
}
