using AthleteDataAccessLibrary.Contracts;
using CoreLibrary.Contracts;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;

using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace AthleteDataAccessLibrary
{
    public class DataAccess : IDataAccess
	{
		private readonly CosmosClient _client;

		public DataAccess(string connectionString)
		{
			var client = new CosmosClientBuilder(connectionString)
								.WithSerializerOptions(new CosmosSerializationOptions
								{
									PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
								})
								.Build();
			_client = client;
			
		}

		public async Task<List<T>> LoadData<T, U>(string cosmosDb, string container, U parameter)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			var iterator = containerAccess.GetItemLinqQueryable<T>().ToFeedIterator();
			List<T> results = new List<T>();

			while (iterator.HasMoreResults)
			{ 
				foreach (var item in await iterator.ReadNextAsync())
				{
					results.Add(item);
				}
			}
			return results;
		}

		public async Task<T> LoadItem<T>(string cosmosDb, string container, string id) where T : IQueryItem
		{
            var containerAccess =  _client.GetContainer(cosmosDb, container);
			var q = containerAccess.GetItemLinqQueryable<T>(true);
			var iterator = q.Where<T>(item => item.Id == id).ToFeedIterator();
			var result = await iterator.ReadNextAsync();
			return result.FirstOrDefault();
        }


		public async Task<T> DeleteItem<T,U>(string cosmosDb, string container, U parameter) 
		{
            var containerAccess = _client.GetContainer(cosmosDb, container);
            return await containerAccess.DeleteItemAsync<T>(parameter as string, PartitionKey.None);
        }


		public async Task SaveData<T>(string comsosDb, string container, T parameter)
		{
			var containerAccess = _client.GetContainer(comsosDb, container);
			await containerAccess.CreateItemAsync(parameter);
			
		}

        public async Task UpsertItem<T>(string cosmosDb, string container, T parameter)
        {
            var containerAccess = _client.GetContainer(cosmosDb, container);
            await containerAccess.UpsertItemAsync<T>(parameter, PartitionKey.None);
        }
    }
}
