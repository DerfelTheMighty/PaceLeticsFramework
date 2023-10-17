using CoreLibrary.Models.Athlet;

namespace AthleteDataAccessLibrary
{

	public interface IAthleteData
	{
		Task<List<AthleteModel>> GetAthletes();
		Task InsertAthlete(AthleteModel model);
		Task<AthleteModel> GetAthlete(int id);
	}
}