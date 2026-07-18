namespace PaceLetics.CoreModule.Infrastructure.Models
{
    public class EnduranceProjectionModel
    {
        public double AnchorDistanceMeters { get; set; }

        public double AnchorPaceSecondsPerKilometer { get; set; }

        public double FatigueExponent { get; set; }

        public bool UsesDefaultExponent { get; set; }

        public bool IsValid => AnchorDistanceMeters > 0
            && AnchorPaceSecondsPerKilometer > 0
            && FatigueExponent > 1;

        public double? PredictPaceSecondsPerKilometer(double distanceMeters)
        {
            if (!IsValid || distanceMeters < AnchorDistanceMeters)
                return null;

            return AnchorPaceSecondsPerKilometer
                * Math.Pow(distanceMeters / AnchorDistanceMeters, FatigueExponent - 1);
        }
    }
}
