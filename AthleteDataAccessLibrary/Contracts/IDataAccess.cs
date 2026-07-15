
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
        Task<List<T>> LoadData<T>(string cosmosDb, string container, CancellationToken cancellationToken = default);

        Task<List<T>> LoadData<T>(string cosmosDb, string container, string documentType, CancellationToken cancellationToken = default);

        Task<List<T>> LoadPartitionData<T>(string cosmosDb, string container, string partitionKeyValue, string documentType, CancellationToken cancellationToken = default);

        Task<List<T>> QueryData<T>(
            string cosmosDb,
            string container,
            string queryText,
            IReadOnlyDictionary<string, object?> parameters,
            string? partitionKeyValue = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="cosmosDb"></param>
        /// <param name="container"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<T?> LoadItem<T>(string cosmosDb, string container, string id, CancellationToken cancellationToken = default) where T : IQueryItem;

        Task<T?> LoadItem<T>(string cosmosDb, string container, string id, string partitionKeyValue, CancellationToken cancellationToken = default) where T : IQueryItem;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cosmosDb"></param>
        /// <param name="container"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteItem<T>(string cosmosDb, string container, string id, CancellationToken cancellationToken = default);

        Task DeleteItem<T>(string cosmosDb, string container, string id, string partitionKeyValue, CancellationToken cancellationToken = default);

        Task DeleteItems(
            string cosmosDb,
            string container,
            IReadOnlyCollection<string> ids,
            string partitionKeyValue,
            CancellationToken cancellationToken = default);


        /// <summary>
        /// Insert or update existing item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cosmosDb"></param>
        /// <param name="containerId"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task UpsertItem<T>(string cosmosDb, string containerId, T parameter, CancellationToken cancellationToken = default);

        Task UpsertItem<T>(string cosmosDb, string containerId, T parameter, string partitionKeyValue, CancellationToken cancellationToken = default);


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cosmosDb"></param>
        /// <param name="containerId"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task SaveData<T>(string cosmosDb, string containerId, T parameter, CancellationToken cancellationToken = default);
    }
}
