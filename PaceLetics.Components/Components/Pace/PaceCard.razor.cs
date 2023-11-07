using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Components.ObjectEdit;
using CoreLibrary.Constants;

namespace PaceLetics.Components.Pace
{
    public partial class PaceCard
    {
        [Parameter]
        public string? PaceKey { get; set; }

        [Parameter]
        public TimeSpan LowerPace { get; set; }

        [Parameter]
        public TimeSpan UpperPace { get; set; }

        [Parameter]
        public EventCallback<string> ShowMoreInfoEvent { get; set; }

        private async Task OnShowMoreInfoClickAsnyc()
        {
            await ShowMoreInfoEvent.InvokeAsync(PaceKey);
        }
    }
}