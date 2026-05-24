
using PaceLetics.CoreModule.Infrastructure.Interfaces;

namespace AthleteDataAccessLibrary.Contracts
{
    public interface IDataAccess
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<List<T>> LoadData<T>(string cosmosDb, string container);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="cosmosDb"></param>
        /// <param name="container"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<T?> LoadItem<T>(string cosmosDb, string container, string id) where T : IQueryItem;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cosmosDb"></param>
        /// <param name="container"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteItem<T>(string cosmosDb, string container, string id);


        /// <summary>
        /// Insert or update existing item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cosmosDb"></param>
        /// <param name="containerId"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task UpsertItem<T>(string cosmosDb, string containerId, T parameter);


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cosmosDb"></param>
        /// <param name="containerId"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task SaveData<T>(string cosmosDb, string containerId, T parameter);
    }
}
