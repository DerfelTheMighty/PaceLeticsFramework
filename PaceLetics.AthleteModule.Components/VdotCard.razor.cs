using Microsoft.AspNetCore.Components;

namespace PaceLetics.AthleteModule.Components
{
    public partial class VdotCard
    {
        private int _index = -1;
        [Parameter]
        public double[] Data { get; set; } = {0, 0};
    }
}