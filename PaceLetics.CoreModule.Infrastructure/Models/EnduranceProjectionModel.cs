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
            var projectedSpeedFraction = InterpolateShapePreserving(
                distanceMeters,
                anchorSpeedFraction,
                1,
                MarathonSpeedFraction);

            return CriticalPaceSecondsPerKilometer / projectedSpeedFraction;
        }

        public double? PredictSpeedFractionError(double distanceMeters)
        {
            if (!IsValid || distanceMeters < AnchorDistanceMeters)
                return null;

            return InterpolateShapePreserving(
                distanceMeters,
                ShortDistanceSpeedErrorFraction,
                CriticalDistanceSpeedErrorFraction,
                MarathonSpeedErrorFraction);
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

        private double InterpolateShapePreserving(
            double distanceMeters,
            double anchorValue,
            double criticalValue,
            double marathonValue)
        {
            var clampedDistance = Math.Clamp(
                distanceMeters,
                AnchorDistanceMeters,
                MarathonDistanceMeters);
            var anchorX = Math.Log(AnchorDistanceMeters);
            var marathonX = Math.Log(MarathonDistanceMeters);
            var x = Math.Log(clampedDistance);

            if (CriticalDistanceMeters <= AnchorDistanceMeters + double.Epsilon)
            {
                var position = (x - anchorX) / (marathonX - anchorX);
                return Lerp(anchorValue, marathonValue, position);
            }

            var criticalX = Math.Log(CriticalDistanceMeters);
            var anchorInterval = criticalX - anchorX;
            var marathonInterval = marathonX - criticalX;
            var anchorSecant = (criticalValue - anchorValue) / anchorInterval;
            var marathonSecant = (marathonValue - criticalValue) / marathonInterval;
            var anchorSlope = CalculateEndpointSlope(
                anchorInterval,
                marathonInterval,
                anchorSecant,
                marathonSecant);
            var criticalSlope = CalculateInteriorSlope(
                anchorInterval,
                marathonInterval,
                anchorSecant,
                marathonSecant);
            var marathonSlope = CalculateEndpointSlope(
                marathonInterval,
                anchorInterval,
                marathonSecant,
                anchorSecant);

            return x <= criticalX
                ? InterpolateHermite(
                    x,
                    anchorX,
                    criticalX,
                    anchorValue,
                    criticalValue,
                    anchorSlope,
                    criticalSlope)
                : InterpolateHermite(
                    x,
                    criticalX,
                    marathonX,
                    criticalValue,
                    marathonValue,
                    criticalSlope,
                    marathonSlope);
        }

        private static double CalculateInteriorSlope(
            double leftInterval,
            double rightInterval,
            double leftSecant,
            double rightSecant)
        {
            if (leftSecant == 0
                || rightSecant == 0
                || Math.Sign(leftSecant) != Math.Sign(rightSecant))
            {
                return 0;
            }

            var leftWeight = 2 * rightInterval + leftInterval;
            var rightWeight = rightInterval + 2 * leftInterval;
            return (leftWeight + rightWeight)
                / (leftWeight / leftSecant + rightWeight / rightSecant);
        }

        private static double CalculateEndpointSlope(
            double edgeInterval,
            double neighborInterval,
            double edgeSecant,
            double neighborSecant)
        {
            var slope = ((2 * edgeInterval + neighborInterval) * edgeSecant
                - edgeInterval * neighborSecant)
                / (edgeInterval + neighborInterval);

            if (Math.Sign(slope) != Math.Sign(edgeSecant))
                return 0;

            if (Math.Sign(edgeSecant) != Math.Sign(neighborSecant)
                && Math.Abs(slope) > Math.Abs(3 * edgeSecant))
            {
                return 3 * edgeSecant;
            }

            return slope;
        }

        private static double InterpolateHermite(
            double x,
            double startX,
            double endX,
            double startValue,
            double endValue,
            double startSlope,
            double endSlope)
        {
            var interval = endX - startX;
            var position = Math.Clamp((x - startX) / interval, 0, 1);
            var positionSquared = position * position;
            var positionCubed = positionSquared * position;
            var startBasis = 2 * positionCubed - 3 * positionSquared + 1;
            var startSlopeBasis = positionCubed - 2 * positionSquared + position;
            var endBasis = -2 * positionCubed + 3 * positionSquared;
            var endSlopeBasis = positionCubed - positionSquared;

            return startBasis * startValue
                + startSlopeBasis * interval * startSlope
                + endBasis * endValue
                + endSlopeBasis * interval * endSlope;
        }

        private static double Lerp(double start, double end, double position) =>
            start + (end - start) * Math.Clamp(position, 0, 1);
    }
}
