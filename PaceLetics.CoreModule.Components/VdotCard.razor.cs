using Microsoft.AspNetCore.Components;

namespace PaceLetics.CoreModule.Components
{
    public partial class VdotCard
    {
        private int _index = -1; //default value cannot be 0 -> first selectedindex is 0.
        [Parameter]
        public double[] Data { get; set; } = {0, 0};
    }
}