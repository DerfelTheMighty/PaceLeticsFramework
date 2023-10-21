using CoreLibrary.Models.Athlet;
using CoreLibrary.Contracts;

namespace AthleteDataAccessLibrary.Contracts
{

    public interface IAthleteData
    {
        /// <summary>
        /// Reads all athletes from the database
        /// </summary>
        /// <returns></returns>
        Task<List<AthleteModel>> GetAthletes();

        /// <summary>
        /// Inserts a new athlete model to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task InsertAthlete(AthleteModel model);

        /// <summary>
        /// Removes the athlete with the given guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task DeleteAthlete(string guid);

        /// <summary>
        /// Returns the athlete model with the given guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AthleteModel> GetAthlete(string id);

        /// <summary>
        /// Updates existing athlete model in database
        /// </summary>
        /// <returns></returns>
        Task UpdateAthlete(AthleteModel model);

    }
}