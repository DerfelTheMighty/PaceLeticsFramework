using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

using PaceLetics.CoreModule.Infrastructure.Constants;
using PaceLetics.VdotModule.CodeBase.Models;

namespace PaceLetics.VdotModule.Components
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
            var imagePath = "/images/icons/epace.png"; // default image
            switch (type)
            {
                case RaceKeys.D1k:
                    imagePath = "/images/icons/icon_1k.png";
                    break;
                case RaceKeys.D3k:
                    imagePath = "/images/icons/icon_3k.png";
                    break;
                case RaceKeys.D5k:
                    imagePath = "/images/icons/icon_5k.png";
                    break;
                case RaceKeys.D10k:
                    imagePath = "/images/icons/icon_10k.png";
                    break;
                case RaceKeys.D15k:
                    imagePath = "/images/icons/icon_15k.png";
                    break;
                case RaceKeys.D21k:
                    imagePath = "/images/icons/icon_21k.png";
                    break;
                default:
                    imagePath = "/images/icons/icon_3k.png";
                    break;
            }

            return imagePath;
        }
    }
}