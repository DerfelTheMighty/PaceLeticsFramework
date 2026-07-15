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

		public Task<List<AthleteModel>> GetPublicAthletes()
		{
			return _db.QueryData<AthleteModel>(
				_options.DatabaseName,
				_options.AthleteContainerName,
				"SELECT * FROM c WHERE c.publicProfile.isProfileVisible = true",
				new Dictionary<string, object?>());
		}

		public async Task<bool> PublicUserNameExists(string normalizedPublicUserName, string? exceptUserId = null)
		{
			var query = string.IsNullOrWhiteSpace(exceptUserId)
				? "SELECT TOP 1 VALUE c.id FROM c WHERE c.publicProfile.normalizedPublicUserName = @name"
				: "SELECT TOP 1 VALUE c.id FROM c WHERE c.publicProfile.normalizedPublicUserName = @name AND c.id != @exceptUserId";
			var parameters = new Dictionary<string, object?>
			{
				["@name"] = normalizedPublicUserName
			};
			if (!string.IsNullOrWhiteSpace(exceptUserId))
				parameters["@exceptUserId"] = exceptUserId;

			var matches = await _db.QueryData<string>(
				_options.DatabaseName,
				_options.AthleteContainerName,
				query,
				parameters);
			return matches.Count > 0;
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
