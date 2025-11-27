using Microsoft.AspNetCore.Components;



namespace PaceLetics.VdotModule.Components
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