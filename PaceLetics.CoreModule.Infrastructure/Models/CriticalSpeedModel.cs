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
    }
}
