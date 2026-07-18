namespace PaceLetics.CoreModule.Infrastructure.Converter
{
    public static class PaceFormatting
    {
        public static TimeSpan FromSpeed(double metersPerSecond)
        {
            return metersPerSecond <= 0
                ? default
                : TimeSpan.FromSeconds(Math.Round(1000 / metersPerSecond));
        }

        public static string FormatFromSpeed(double metersPerSecond)
        {
            var pace = FromSpeed(metersPerSecond);
            return pace == default
                ? "-"
                : $"{(int)pace.TotalMinutes}:{pace.Seconds:00} min/km";
        }
    }
}
