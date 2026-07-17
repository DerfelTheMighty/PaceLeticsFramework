using Microsoft.AspNetCore.Components;
using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.AthleteModule.Components
{
    public partial class RaceCard
    {
        [Parameter]
        public RaceResultModel? Model { get; set; }

        [Parameter]
        public EventCallback<RaceResultModel> OnEditRaceCard { get; set; }

        [Parameter]
        public EventCallback<RaceResultModel> OnDeleteRaceCard { get; set; }

        private async Task HandleEditCard()
        {
            if (Model is not null)
                await OnEditRaceCard.InvokeAsync(Model);
        }

        private async Task HandleDeleteCard()
        {
            if (Model is not null)
                await OnDeleteRaceCard.InvokeAsync(Model);
        }

        private static string GetRaceDistanceCode(string? type)
        {
            return type switch
            {
                RaceKeys.D1k => "1K",
                RaceKeys.D1200m => "1200",
                RaceKeys.D3k => "3K",
                RaceKeys.D3600m => "3600",
                RaceKeys.D5k => "5K",
                RaceKeys.D10k => "10K",
                RaceKeys.D15k => "15K",
                RaceKeys.D21k => "21K",
                RaceKeys.D42k => "42K",
                _ => "RUN"
            };
        }

        private static string GetRaceDistanceClass(string? type)
        {
            return type switch
            {
                RaceKeys.D1k => "pl-race-distance-1k",
                RaceKeys.D1200m => "pl-race-distance-1k",
                RaceKeys.D3k => "pl-race-distance-3k",
                RaceKeys.D3600m => "pl-race-distance-3k",
                RaceKeys.D5k => "pl-race-distance-5k",
                RaceKeys.D10k => "pl-race-distance-10k",
                RaceKeys.D15k => "pl-race-distance-15k",
                RaceKeys.D21k => "pl-race-distance-21k",
                RaceKeys.D42k => "pl-race-distance-42k",
                _ => "pl-race-distance-default"
            };
        }
    }
}
