using AthleteDataAccessLibrary.Contracts;
using PaceLetics.AthleteModule.CodeBase.Models;

namespace AthleteDataAccessLibrary
{
	public class AthleteData : IAthleteData
	{
		private readonly IDataAccess _db;
		private readonly AthleteDataOptions _options;

		public AthleteData(IDataAccess db, AthleteDataOptions options)
		{
			_db = db;
			_options = options;
			_options.Validate();
		}

		public Task<List<AthleteModel>> GetAthletes()
		{
			return _db.LoadData<AthleteModel>(_options.DatabaseName, _options.AthleteContainerName);
		}

		
		public Task InsertAthlete(AthleteModel model)
		{
			return _db.SaveData(_options.DatabaseName, _options.AthleteContainerName, model);
		}


		public Task<AthleteModel?> GetAthlete(string id)
		{
            return _db.LoadItem<AthleteModel>(_options.DatabaseName, _options.AthleteContainerName, id);
        }

		public Task DeleteAthlete(string id) 
		{
			return _db.DeleteItem<AthleteModel>(_options.DatabaseName, _options.AthleteContainerName, id);
        }

		public Task UpdateAthlete(AthleteModel model) 
		{
			return _db.UpsertItem(_options.DatabaseName, _options.AthleteContainerName, model);
		}

    }
}
