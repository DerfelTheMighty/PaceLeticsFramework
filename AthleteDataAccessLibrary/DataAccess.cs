using Azure;
using CoreLibrary.Models.Athlet;
using CoreLibrary.Models.Contracts;
using Dapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Extensions.Configuration;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace AthleteDataAccessLibrary
{
	public class DataAccess : IDataAccess
	{
		private readonly IConfiguration _config;
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

		public async Task<T> LoadItem<T>(string cosmosDb, string container, string id, PartitionKey key) where T : IQueryItem
		{
            var containerAccess = _client.GetContainer(cosmosDb, container);
            return containerAccess.GetItemLinqQueryable<T>(true).Where<T>(item => item.Id == id).ToArray().FirstOrDefault(); 
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

	}
}
