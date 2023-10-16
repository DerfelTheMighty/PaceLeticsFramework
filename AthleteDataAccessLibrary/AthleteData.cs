using CoreLibrary.Models.Athlet;

namespace AthleteDataAccessLibrary
{
	public class AthleteData : IAthleteData
	{
		private readonly IDataAccess _db;
		private readonly string _cosmosDb = "paceleticsdata";
		private readonly string _containerId = "athletedata";
		public AthleteData(IDataAccess db)
		{
			_db = db;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Task<List<AthleteModel>> GetAthletes()
		{
			return _db.LoadData<AthleteModel, dynamic>(_cosmosDb, _containerId, new { });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public Task InsertAthlete(AthleteModel model)
		{
			return _db.SaveData(_cosmosDb, _containerId, model);
		}
	}
}
