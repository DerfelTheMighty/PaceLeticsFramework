using Microsoft.AspNetCore.Components;


namespace PaceLetics.AthleteModule.Components
{
    public partial class PaceInfo
    {
        public record PaceInfoItem(
            string Title,
            string ZoneKey,
            string Code,
            string PaceText,
            string Description);

        [Parameter] public TimeSpan EPaceLow { get; set; }
        [Parameter] public TimeSpan EPaceHigh { get; set; }

        [Parameter] public TimeSpan RecoveryPaceLow { get; set; }
        [Parameter] public TimeSpan RecoveryPaceHigh { get; set; }

        [Parameter] public TimeSpan MPaceLow { get; set; }
        [Parameter] public TimeSpan MPaceHigh { get; set; }

        [Parameter] public TimeSpan TPaceLow { get; set; }
        [Parameter] public TimeSpan TPaceHigh { get; set; }

        [Parameter] public TimeSpan IPaceLow { get; set; }
        [Parameter] public TimeSpan IPaceHigh { get; set; }

        [Parameter] public TimeSpan RPaceLow { get; set; }
        [Parameter] public TimeSpan RPaceHigh { get; set; }

        private List<PaceInfoItem> Items = new();

        protected override void OnParametersSet()
        {
            var recoveryLow = RecoveryPaceLow == default
                ? EPaceLow.Add(TimeSpan.FromSeconds(30))
                : RecoveryPaceLow;
            var recoveryHigh = RecoveryPaceHigh == default
                ? EPaceHigh.Add(TimeSpan.FromSeconds(30))
                : RecoveryPaceHigh;

            Items = new()
            {
                new(L["PaceInfo_Free_Title"],
                    ZoneKey: "free",
                    Code: "Free",
                    PaceText: L["PaceInfo_Free_Value"],
                    Description: L["PaceInfo_Free_Description"]),

                new(L["PaceInfo_Recovery_Title"],
                    ZoneKey: "recovery",
                    Code: "Rec",
                    PaceText: FormatPaceRange(recoveryHigh, recoveryLow),
                    Description: L["PaceInfo_Recovery_Description"]),

                new(L["PaceInfo_EPace_Title"],
                    ZoneKey: "easy",
                    Code: "E",
                    PaceText: FormatPaceRange(EPaceHigh, EPaceLow),
                    Description: L["PaceInfo_EPace_Description"]),

                new(L["PaceInfo_MPace_Title"],
                    ZoneKey: "marathon",
                    Code: "M",
                    PaceText: FormatPaceRange(MPaceHigh, MPaceLow),
                    Description: L["PaceInfo_MPace_Description"]),

                new(L["PaceInfo_TPace_Title"],
                    ZoneKey: "threshold",
                    Code: "T",
                    PaceText: FormatPaceRange(TPaceHigh, TPaceLow),
                    Description: L["PaceInfo_TPace_Description"]),

                new(L["PaceInfo_IPace_Title"],
                    ZoneKey: "intervall",
                    Code: "I",
                    PaceText: FormatPaceRange(IPaceHigh, IPaceLow),
                    Description: L["PaceInfo_IPace_Description"]),

                new(L["PaceInfo_RPace_Title"],
                    ZoneKey: "repetition",
                    Code: "R",
                    PaceText: FormatPaceRange(RPaceHigh, RPaceLow),
                    Description: L["PaceInfo_RPace_Description"])
            };
        }

        private static string FormatPaceRange(TimeSpan upper, TimeSpan lower)
        {
            return $"{upper:mm\\:ss} - {lower:mm\\:ss} min/km";
        }
    }
}
