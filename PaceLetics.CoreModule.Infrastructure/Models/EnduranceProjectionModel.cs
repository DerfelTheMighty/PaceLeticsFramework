namespace PaceLetics.CoreModule.Infrastructure.Models
{
    public readonly record struct EnduranceProjectionPaceRange(
        double FasterPaceSecondsPerKilometer,
        double SlowerPaceSecondsPerKilometer);

    public class EnduranceProjectionModel
    {
        public double AnchorDistanceMeters { get; set; }

        public double AnchorPaceSecondsPerKilometer { get; set; }

        public double CriticalPaceSecondsPerKilometer { get; set; }

        public double CriticalDistanceMeters { get; set; }

        public double MarathonDistanceMeters { get; set; }

        public double MarathonSpeedFraction { get; set; }

        public double ShortDistanceSpeedErrorFraction { get; set; }

        public double CriticalDistanceSpeedErrorFraction { get; set; }

        public double MarathonSpeedErrorFraction { get; set; }

        public bool IsValid => AnchorDistanceMeters > 0
            && AnchorPaceSecondsPerKilometer > 0
            && CriticalPaceSecondsPerKilometer > 0
            && CriticalDistanceMeters >= AnchorDistanceMeters
            && MarathonDistanceMeters > CriticalDistanceMeters
            && MarathonSpeedFraction is > 0 and < 1
            && ShortDistanceSpeedErrorFraction >= 0
            && CriticalDistanceSpeedErrorFraction >= ShortDistanceSpeedErrorFraction
            && MarathonSpeedErrorFraction >= CriticalDistanceSpeedErrorFraction;

        public double? PredictPaceSecondsPerKilometer(double distanceMeters)
        {
            if (!IsValid || distanceMeters < AnchorDistanceMeters)
                return null;

            var anchorSpeedFraction = CriticalPaceSecondsPerKilometer
                / AnchorPaceSecondsPerKilometer;

            if (distanceMeters < CriticalDistanceMeters
                && CriticalDistanceMeters > AnchorDistanceMeters)
            {
                var position = (distanceMeters - AnchorDistanceMeters)
                    / (CriticalDistanceMeters - AnchorDistanceMeters);
                var speedFraction = Interpolate(anchorSpeedFraction, 1, SmoothStep(position));
                return CriticalPaceSecondsPerKilometer / speedFraction;
            }

            var enduranceStartDistance = Math.Max(AnchorDistanceMeters, CriticalDistanceMeters);
            var enduranceStartFraction = AnchorDistanceMeters >= CriticalDistanceMeters
                ? anchorSpeedFraction
                : 1;
            var logarithmicRange = Math.Log(MarathonDistanceMeters / enduranceStartDistance);
            var logarithmicPosition = logarithmicRange <= double.Epsilon
                ? 1
                : Math.Log(distanceMeters / enduranceStartDistance) / logarithmicRange;
            var marathonProgress = SmoothStep(Math.Clamp(logarithmicPosition, 0, 1));
            var projectedSpeedFraction = Interpolate(
                enduranceStartFraction,
                MarathonSpeedFraction,
                marathonProgress);

            return CriticalPaceSecondsPerKilometer / projectedSpeedFraction;
        }

        public double? PredictSpeedFractionError(double distanceMeters)
        {
            if (!IsValid || distanceMeters < AnchorDistanceMeters)
                return null;

            if (distanceMeters < CriticalDistanceMeters
                && CriticalDistanceMeters > AnchorDistanceMeters)
            {
                var position = (distanceMeters - AnchorDistanceMeters)
                    / (CriticalDistanceMeters - AnchorDistanceMeters);
                return Interpolate(
                    ShortDistanceSpeedErrorFraction,
                    CriticalDistanceSpeedErrorFraction,
                    SmoothStep(position));
            }

            var logarithmicRange = Math.Log(MarathonDistanceMeters / CriticalDistanceMeters);
            var logarithmicPosition = logarithmicRange <= double.Epsilon
                ? 1
                : Math.Log(distanceMeters / CriticalDistanceMeters) / logarithmicRange;
            return Interpolate(
                CriticalDistanceSpeedErrorFraction,
                MarathonSpeedErrorFraction,
                SmoothStep(Math.Clamp(logarithmicPosition, 0, 1)));
        }

        public EnduranceProjectionPaceRange? PredictPaceRange(double distanceMeters)
        {
            var projectedPace = PredictPaceSecondsPerKilometer(distanceMeters);
            var speedFractionError = PredictSpeedFractionError(distanceMeters);
            if (projectedPace is null || speedFractionError is null)
                return null;

            var projectedSpeedFraction = CriticalPaceSecondsPerKilometer / projectedPace.Value;
            var fasterSpeedFraction = projectedSpeedFraction + speedFractionError.Value;
            var slowerSpeedFraction = Math.Max(0.01, projectedSpeedFraction - speedFractionError.Value);

            return new EnduranceProjectionPaceRange(
                CriticalPaceSecondsPerKilometer / fasterSpeedFraction,
                CriticalPaceSecondsPerKilometer / slowerSpeedFraction);
        }

        private static double Interpolate(double start, double end, double position) =>
            start + (end - start) * position;

        private static double SmoothStep(double position) =>
            position * position * (3 - 2 * position);
    }
}
