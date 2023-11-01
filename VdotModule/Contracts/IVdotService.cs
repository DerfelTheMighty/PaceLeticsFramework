using CoreLibrary.Models.Race;


namespace VdotModule.Contracts
{
    public interface IVdotService
    {
        double GetVdot(RaceResultModel result);        
    }
}
