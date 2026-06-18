using Microsoft.AspNetCore.Components;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.AthleteModule.Components
{
    public partial class PaceComparisonInfo
    {
        private const string MissingValue = "-";

        public record PaceComparisonItem(
            string Title,
            string ZoneKey,
            string Code,
            string DanielsPaceText,
            string CriticalSpeedPaceText,
            string Description);

        [Parameter] public PaceModel DanielsUpper { get; set; } = new();
        [Parameter] public PaceModel DanielsLower { get; set; } = new();
        [Parameter] public bool HasDaniels { get; set; }

        [Parameter] public PaceModel CriticalSpeedUpper { get; set; } = new();
        [Parameter] public PaceModel CriticalSpeedLower { get; set; } = new();
        [Parameter] public bool HasCriticalSpeed { get; set; }

        private List<PaceComparisonItem> Items = new();

        protected override void OnParametersSet()
        {
            Items = new()
            {
                new(L["PaceInfo_Free_Title"],
                    ZoneKey: "free",
                    Code: "Free",
                    DanielsPaceText: L["PaceInfo_Free_Value"],
                    CriticalSpeedPaceText: L["PaceInfo_Free_Value"],
                    Description: L["PaceInfo_Free_Description"]),

                new(L["PaceInfo_Recovery_Title"],
                    ZoneKey: "recovery",
                    Code: "Rec",
                    DanielsPaceText: FormatPaceRange(HasDaniels, DanielsUpper.Easy.Add(TimeSpan.FromSeconds(30)), DanielsLower.Easy.Add(TimeSpan.FromSeconds(30))),
                    CriticalSpeedPaceText: FormatPaceRange(HasCriticalSpeed, CriticalSpeedUpper.Easy.Add(TimeSpan.FromSeconds(30)), CriticalSpeedLower.Easy.Add(TimeSpan.FromSeconds(30))),
                    Description: L["PaceInfo_Recovery_Description"]),

                new(L["PaceInfo_EPace_Title"],
                    ZoneKey: "easy",
                    Code: "E",
                    DanielsPaceText: FormatPaceRange(HasDaniels, DanielsUpper.Easy, DanielsLower.Easy),
                    CriticalSpeedPaceText: FormatPaceRange(HasCriticalSpeed, CriticalSpeedUpper.Easy, CriticalSpeedLower.Easy),
                    Description: L["PaceInfo_EPace_Description"]),

                new(L["PaceInfo_MPace_Title"],
                    ZoneKey: "marathon",
                    Code: "M",
                    DanielsPaceText: FormatPaceRange(HasDaniels, DanielsUpper.Marathon, DanielsLower.Marathon),
                    CriticalSpeedPaceText: FormatPaceRange(HasCriticalSpeed, CriticalSpeedUpper.Marathon, CriticalSpeedLower.Marathon),
                    Description: L["PaceInfo_MPace_Description"]),

                new(L["PaceInfo_TPace_Title"],
                    ZoneKey: "threshold",
                    Code: "T",
                    DanielsPaceText: FormatPaceRange(HasDaniels, DanielsUpper.Threshold, DanielsLower.Threshold),
                    CriticalSpeedPaceText: FormatPaceRange(HasCriticalSpeed, CriticalSpeedUpper.Threshold, CriticalSpeedLower.Threshold),
                    Description: L["PaceInfo_TPace_Description"]),

                new(L["PaceInfo_IPace_Title"],
                    ZoneKey: "intervall",
                    Code: "I",
                    DanielsPaceText: FormatPaceRange(HasDaniels, DanielsUpper.Intervall, DanielsLower.Intervall),
                    CriticalSpeedPaceText: FormatPaceRange(HasCriticalSpeed, CriticalSpeedUpper.Intervall, CriticalSpeedLower.Intervall),
                    Description: L["PaceInfo_IPace_Description"]),

                new(L["PaceInfo_RPace_Title"],
                    ZoneKey: "repetition",
                    Code: "R",
                    DanielsPaceText: FormatPaceRange(HasDaniels, DanielsUpper.Repetition, DanielsLower.Repetition),
                    CriticalSpeedPaceText: FormatPaceRange(HasCriticalSpeed, CriticalSpeedUpper.Repetition, CriticalSpeedLower.Repetition),
                    Description: L["PaceInfo_RPace_Description"])
            };
        }

        private static string FormatPaceRange(bool hasModel, TimeSpan upper, TimeSpan lower)
        {
            if (!hasModel || upper == default || lower == default)
                return MissingValue;

            return $"{upper:mm\\:ss} - {lower:mm\\:ss} min/km";
        }
    }
}
