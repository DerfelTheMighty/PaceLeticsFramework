using Microsoft.AspNetCore.Components;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.AthleteModule.Components
{
    public partial class CriticalSpeedBudgetInfo
    {
        private record BudgetZone(
            string Title,
            string ZoneKey,
            string Code,
            string PaceText,
            string BudgetText,
            string Description);

        [Parameter] public CriticalSpeedModel Model { get; set; } = new();

        [Parameter] public IReadOnlyList<CriticalSpeedIntervalRecommendation> Recommendations { get; set; } = [];

        private List<BudgetZone> Zones { get; set; } = [];

        private List<CriticalSpeedIntervalRecommendation> LongIntervals { get; set; } = [];

        private List<CriticalSpeedIntervalRecommendation> FastIntervals { get; set; } = [];

        protected override void OnParametersSet()
        {
            Zones = Model.IsValid
                ? BuildZones()
                : [];

            LongIntervals = Recommendations
                .Where(item => !item.IsFastInterval)
                .OrderBy(item => item.DistanceMeters)
                .ToList();

            FastIntervals = Recommendations
                .Where(item => item.IsFastInterval)
                .OrderBy(item => item.DistanceMeters)
                .ToList();
        }

        private List<BudgetZone> BuildZones()
        {
            return
            [
                new(L["CriticalSpeedBudget_Recovery_Title"],
                    ZoneKey: "recovery",
                    Code: "Rec",
                    PaceText: FormatPaceRange(Model.CriticalSpeedMps * 0.82, Model.CriticalSpeedMps * 0.70),
                    BudgetText: L["CriticalSpeedBudget_NoDPrime"],
                    Description: L["CriticalSpeedBudget_Recovery_Description"]),

                new(L["CriticalSpeedBudget_Easy_Title"],
                    ZoneKey: "easy",
                    Code: "E",
                    PaceText: FormatPaceRange(Model.CriticalSpeedMps * 0.92, Model.CriticalSpeedMps * 0.82),
                    BudgetText: L["CriticalSpeedBudget_NoDPrime"],
                    Description: L["CriticalSpeedBudget_Easy_Description"]),

                new(L["CriticalSpeedBudget_Threshold_Title"],
                    ZoneKey: "threshold",
                    Code: "T",
                    PaceText: FormatPaceRange(Model.CriticalSpeedMps, Model.CriticalSpeedMps * 0.96),
                    BudgetText: L["CriticalSpeedBudget_NoDPrime"],
                    Description: L["CriticalSpeedBudget_Threshold_Description"]),

                new(L["CriticalSpeedBudget_LongZone_Title"],
                    ZoneKey: "intervall",
                    Code: "Int",
                    PaceText: L["CriticalSpeedBudget_TableReference"],
                    BudgetText: L["CriticalSpeedBudget_LongZone_Budget"],
                    Description: L["CriticalSpeedBudget_LongZone_Description"]),

                new(L["CriticalSpeedBudget_FastZone_Title"],
                    ZoneKey: "repetition",
                    Code: "Fast",
                    PaceText: L["CriticalSpeedBudget_TableReference"],
                    BudgetText: L["CriticalSpeedBudget_FastZone_Budget"],
                    Description: L["CriticalSpeedBudget_FastZone_Description"])
            ];
        }

        private static string FormatPaceRange(double fastSpeedMps, double slowSpeedMps)
        {
            return $"{FormatPace(PaceFromSpeed(fastSpeedMps))} - {FormatPace(PaceFromSpeed(slowSpeedMps))}";
        }

        private static string FormatPace(TimeSpan pace)
        {
            return pace == default
                ? "-"
                : $"{pace:mm\\:ss} min/km";
        }

        private static string FormatDuration(TimeSpan duration)
        {
            return duration.TotalMinutes >= 1
                ? duration.ToString(@"m\:ss")
                : duration.ToString(@"s") + "s";
        }

        private static string FormatDPrime(double? dPrimeMeters)
        {
            return dPrimeMeters is > 0
                ? $"{dPrimeMeters.Value:0} m"
                : "-";
        }

        private static string FormatBudget(CriticalSpeedIntervalRecommendation item)
        {
            return $"D' {item.DPrimeUseMeters:0} m ({item.DPrimeUsePercent:P0})";
        }

        private static TimeSpan PaceFromSpeed(double metersPerSecond)
        {
            return metersPerSecond <= 0
                ? default
                : TimeSpan.FromSeconds(Math.Round(1000 / metersPerSecond));
        }
    }
}
