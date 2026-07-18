using PaceLetics.CoreModule.Infrastructure.Enums;

namespace PaceLetics.CoreModule.Infrastructure.Models
{
    public class CriticalSpeedModel
    {
        public double CriticalSpeedMps { get; set; }

        public double? DPrimeMeters { get; set; }

        public double? IntervalSpeedMps { get; set; }

        public double? RepetitionSpeedMps { get; set; }

        public bool Estimated { get; set; } = true;

        public CriticalSpeedMethod Method { get; set; } = CriticalSpeedMethod.None;

        public int SourceResultCount { get; set; }

        public bool IsValid => CriticalSpeedMps > 0;

        public double? PredictPaceSecondsPerKilometer(double distanceMeters)
        {
            var dPrimeMeters = Math.Max(0, DPrimeMeters ?? 0);
            if (!IsValid || distanceMeters <= dPrimeMeters)
                return null;

            var predictedTimeSeconds = (distanceMeters - dPrimeMeters) / CriticalSpeedMps;
            return predictedTimeSeconds * 1000 / distanceMeters;
        }
    }
}
