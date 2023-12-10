using Microsoft.AspNetCore.Components;
using MudBlazor;


namespace PaceLetics.VdotModule.Components
{
    public partial class PaceInfo
    {
        private MudCarousel<object> _carousel;
        private bool _arrows = true;
        private bool _bullets = false;
        private bool _enableSwipeGesture = true;
        private bool _autocycle = false;
        private Transition _transition = Transition.Slide;
        [Parameter]
        public int SelectedIndex { get; set; }
    }
}