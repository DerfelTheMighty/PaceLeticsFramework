using AthleteDataAccessLibrary.Contracts;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace AthleteDataAccessLibrary
{
    public class DataAccess : IDataAccess
	{
		private readonly CosmosClient _client;

		public DataAccess(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Cosmos DB connection string must be configured.", nameof(connectionString));

			var client = new CosmosClientBuilder(connectionString)
								.WithSerializerOptions(new CosmosSerializationOptions
								{
									PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
								})
								.Build();
			_client = client;
			
		}

		public async Task<List<T>> LoadData<T>(string cosmosDb, string container)
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

		public Task<List<T>> LoadData<T>(string cosmosDb, string container, string documentType)
		{
			var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = @documentType")
				.WithParameter("@documentType", documentType);

			return QueryItems<T>(cosmosDb, container, query);
		}

		public Task<List<T>> LoadPartitionData<T>(string cosmosDb, string container, string partitionKeyValue, string documentType)
		{
			var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = @documentType")
				.WithParameter("@documentType", documentType);

			return QueryItems<T>(
				cosmosDb,
				container,
				query,
				new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKeyValue) });
		}

		public async Task<T?> LoadItem<T>(string cosmosDb, string container, string id) where T : IQueryItem
		{
            var containerAccess =  _client.GetContainer(cosmosDb, container);
			var q = containerAccess.GetItemLinqQueryable<T>(true);
			var iterator = q.Where<T>(item => item.Id == id).ToFeedIterator();
			var result = await iterator.ReadNextAsync();
			return result.FirstOrDefault();
        }

		public async Task<T?> LoadItem<T>(string cosmosDb, string container, string id, string partitionKeyValue) where T : IQueryItem
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);

			try
			{
				var response = await containerAccess.ReadItemAsync<T>(id, new PartitionKey(partitionKeyValue));
				return response.Resource;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return default;
			}
		}


		public async Task DeleteItem<T>(string cosmosDb, string container, string id) 
		{
            var containerAccess = _client.GetContainer(cosmosDb, container);
            await containerAccess.DeleteItemAsync<T>(id, PartitionKey.None);
        }

		public async Task DeleteItem<T>(string cosmosDb, string container, string id, string partitionKeyValue)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			await containerAccess.DeleteItemAsync<T>(id, new PartitionKey(partitionKeyValue));
		}


		public async Task SaveData<T>(string comsosDb, string container, T parameter)
		{
			var containerAccess = _client.GetContainer(comsosDb, container);
			await containerAccess.CreateItemAsync(parameter);
			
		}

        public async Task UpsertItem<T>(string cosmosDb, string container, T parameter)
        {
            var containerAccess = _client.GetContainer(cosmosDb, container);
            await containerAccess.UpsertItemAsync(parameter);
        }

		public async Task UpsertItem<T>(string cosmosDb, string container, T parameter, string partitionKeyValue)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			await containerAccess.UpsertItemAsync(parameter, new PartitionKey(partitionKeyValue));
		}

		private async Task<List<T>> QueryItems<T>(
			string cosmosDb,
			string container,
			QueryDefinition query,
			QueryRequestOptions? options = null)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			var iterator = containerAccess.GetItemQueryIterator<T>(query, requestOptions: options);
			var results = new List<T>();

			while (iterator.HasMoreResults)
			{
				foreach (var item in await iterator.ReadNextAsync())
				{
					results.Add(item);
				}
			}

			return results;
		}
    }
}
