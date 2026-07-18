namespace PaceLetics.CoreModule.Infrastructure.Constants
{
    public static class PaceKeys
    {
        public const string Walk = "Walk";
        public const string Recovery = "Recovery";
        public const string Easy = "Easy";
        public const string Threshold = "Threshold";
        public const string Intervall = "Intervall";
        public const string FastIntervall = "Fast Intervall";
        public const string Free = "Free";

        public static string? Normalize(string? paceKey)
        {
            return paceKey?.Trim() switch
            {
                "Walk" or "Walking" => Walk,
                "Recovery" or "Recovery Pace" => Recovery,
                "Easy" or "E Pace" or "M Pace" => Easy,
                "Threshold" or "T Pace" => Threshold,
                "Intervall" or "Interval" or "I Pace" => Intervall,
                "Fast Intervall" or "Fast Interval" or "R Pace" => FastIntervall,
                "Free" or "Free Pace" => Free,
                null or "" => paceKey,
                _ => paceKey
            };
        }
    }
}
