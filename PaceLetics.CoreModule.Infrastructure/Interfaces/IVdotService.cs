using PaceLetics.CoreModule.Infrastructure.Models.Vdot;

namespace PaceLetics.VdotModule.CodeBase.Interfaces
{
    public interface IVdotService
    {
        double GetVdot(RaceResultModel result);        
    }
}
