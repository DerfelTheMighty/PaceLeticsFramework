using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Components.ObjectEdit;
using CoreLibrary.Constants;
using CoreLibrary.Models.Race;
using MudBlazor;

namespace PaceLetics.Components.Race
{
    public partial class RaceCard
    {
        [Parameter]
        public RaceResultModel? Model { get; set; }

        [Parameter]
        public EventCallback OnEditRaceCard { get; set; }

        private async Task HandleEditCard()
        {
            await OnEditRaceCard.InvokeAsync();
        }

        private string GetImagePath(string type)
        {
            var imagePath = "_content/PaceLetics.Components/images/icons/epace.png"; // default image
            switch (type)
            {
                case RaceKeys.D1k:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_1k.png";
                    break;
                case RaceKeys.D3k:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_3k.png";
                    break;
                case RaceKeys.D5k:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_5k.png";
                    break;
                case RaceKeys.D10k:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_10k.png";
                    break;
                case RaceKeys.D15k:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_15k.png";
                    break;
                case RaceKeys.D21k:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_21k.png";
                    break;
                default:
                    imagePath = "_content/PaceLetics.Components/images/icons/icon_3k.png";
                    break;
            }

            return imagePath;
        }
    }
}