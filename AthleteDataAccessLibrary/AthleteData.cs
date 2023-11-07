using AthleteDataAccessLibrary.Contracts;
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

		public Task<List<AthleteModel>> GetAthletes()
		{
			return _db.LoadData<AthleteModel, dynamic>(_cosmosDb, _containerId, new { });
		}

		
		public Task InsertAthlete(AthleteModel model)
		{
			return _db.SaveData(_cosmosDb, _containerId, model);
		}


		public Task<AthleteModel> GetAthlete(string id)
		{
            return _db.LoadItem<AthleteModel>(_cosmosDb, _containerId, id);
        }

		public Task DeleteAthlete(string id) 
		{
			return _db.DeleteItem<AthleteModel,string>(_cosmosDb, _containerId, id);
        }

		public Task UpdateAthlete(AthleteModel model) 
		{
			return _db.UpsertItem(_cosmosDb, _containerId, model);
		}

    }
}
