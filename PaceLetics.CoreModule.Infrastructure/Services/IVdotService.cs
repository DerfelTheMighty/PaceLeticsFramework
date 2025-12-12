using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Interfaces
{
    public interface IVdotService
    {
        double GetVdot(RaceResultModel result);        
    }
}
