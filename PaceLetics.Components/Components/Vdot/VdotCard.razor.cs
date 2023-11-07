using Microsoft.AspNetCore.Components;

namespace PaceLetics.Components.Vdot
{
    public partial class VdotCard
    {
        private int _index = -1; //default value cannot be 0 -> first selectedindex is 0.
        [Parameter]
        public double[] Data { get; set; } = {0, 0};
    }
}