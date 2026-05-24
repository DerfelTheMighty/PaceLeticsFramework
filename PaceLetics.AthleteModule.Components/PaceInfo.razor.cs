using Microsoft.AspNetCore.Components;


namespace PaceLetics.AthleteModule.Components
{
    public partial class PaceInfo
    {
        public record PaceInfoItem(
         string Title,
         string Icon,
         TimeSpan Upper,
         TimeSpan Lower,
         string Description
     );

        [Parameter] public TimeSpan EPaceLow { get; set; }
        [Parameter] public TimeSpan EPaceHigh { get; set; }

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
            Items = new()
        {
            new(L["PaceInfo_EPace_Title"], "/images/icons/epace.png",
                Upper: EPaceHigh, Lower: EPaceLow,
                Description: L["PaceInfo_EPace_Description"]),

            new(L["PaceInfo_MPace_Title"], "/images/icons/mpace.png",
                Upper: MPaceHigh, Lower: MPaceLow,
                Description: L["PaceInfo_MPace_Description"]),

            new(L["PaceInfo_TPace_Title"], "/images/icons/tpace.png",
                Upper: TPaceHigh, Lower: TPaceLow,
                Description: L["PaceInfo_TPace_Description"]),

            new(L["PaceInfo_IPace_Title"], "/images/icons/ipace.png",
                Upper: IPaceHigh, Lower: IPaceLow,
                Description: L["PaceInfo_IPace_Description"]),

            new(L["PaceInfo_RPace_Title"], "/images/icons/rpace.png",
                Upper: RPaceHigh, Lower: RPaceLow,
                Description: L["PaceInfo_RPace_Description"])
        };
        }
    }
}
