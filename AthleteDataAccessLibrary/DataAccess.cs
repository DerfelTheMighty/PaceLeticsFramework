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

		public DataAccess(CosmosClient client)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
		}

		public static CosmosClient CreateClient(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Cosmos DB connection string must be configured.", nameof(connectionString));

			return new CosmosClientBuilder(connectionString)
				.WithSerializerOptions(new CosmosSerializationOptions
				{
					PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
				})
				.WithConnectionModeDirect()
				.Build();
		}

		public async Task<List<T>> LoadData<T>(string cosmosDb, string container, CancellationToken cancellationToken = default)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			var iterator = containerAccess.GetItemLinqQueryable<T>().ToFeedIterator();
			List<T> results = new List<T>();

			while (iterator.HasMoreResults)
			{ 
				foreach (var item in await iterator.ReadNextAsync(cancellationToken))
				{
					results.Add(item);
				}
			}
			return results;
		}

		public Task<List<T>> LoadData<T>(string cosmosDb, string container, string documentType, CancellationToken cancellationToken = default)
		{
			var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = @documentType")
				.WithParameter("@documentType", documentType);

			return QueryItems<T>(cosmosDb, container, query, cancellationToken: cancellationToken);
		}

		public Task<List<T>> LoadPartitionData<T>(string cosmosDb, string container, string partitionKeyValue, string documentType, CancellationToken cancellationToken = default)
		{
			var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = @documentType")
				.WithParameter("@documentType", documentType);

			return QueryItems<T>(
				cosmosDb,
				container,
				query,
				new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKeyValue) },
				cancellationToken);
		}

		public Task<List<T>> QueryData<T>(
			string cosmosDb,
			string container,
			string queryText,
			IReadOnlyDictionary<string, object?> parameters,
			string? partitionKeyValue = null,
			CancellationToken cancellationToken = default)
		{
			var query = new QueryDefinition(queryText);
			foreach (var parameter in parameters)
				query.WithParameter(parameter.Key, parameter.Value);

			var options = string.IsNullOrWhiteSpace(partitionKeyValue)
				? null
				: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKeyValue) };

			return QueryItems<T>(cosmosDb, container, query, options, cancellationToken);
		}

		public async Task<T?> LoadItem<T>(string cosmosDb, string container, string id, CancellationToken cancellationToken = default) where T : IQueryItem
		{
            var containerAccess =  _client.GetContainer(cosmosDb, container);
			var q = containerAccess.GetItemLinqQueryable<T>(true);
			var iterator = q.Where<T>(item => item.Id == id).ToFeedIterator();
			var result = await iterator.ReadNextAsync(cancellationToken);
			return result.FirstOrDefault();
        }

		public async Task<T?> LoadItem<T>(string cosmosDb, string container, string id, string partitionKeyValue, CancellationToken cancellationToken = default) where T : IQueryItem
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);

			try
			{
				var response = await containerAccess.ReadItemAsync<T>(id, new PartitionKey(partitionKeyValue), cancellationToken: cancellationToken);
				if (response.Resource is IVersionedQueryItem versioned)
					versioned.ETag = response.ETag;
				return response.Resource;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return default;
			}
		}


		public async Task DeleteItem<T>(string cosmosDb, string container, string id, CancellationToken cancellationToken = default)
		{
            var containerAccess = _client.GetContainer(cosmosDb, container);
            await containerAccess.DeleteItemAsync<T>(id, PartitionKey.None, cancellationToken: cancellationToken);
        }

		public async Task DeleteItem<T>(string cosmosDb, string container, string id, string partitionKeyValue, CancellationToken cancellationToken = default)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			await containerAccess.DeleteItemAsync<T>(id, new PartitionKey(partitionKeyValue), cancellationToken: cancellationToken);
		}

		public async Task DeleteItems(
			string cosmosDb,
			string container,
			IReadOnlyCollection<string> ids,
			string partitionKeyValue,
			CancellationToken cancellationToken = default)
		{
			if (ids.Count == 0)
				return;

			var containerAccess = _client.GetContainer(cosmosDb, container);
			foreach (var chunk in ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().Chunk(100))
			{
				var batch = containerAccess.CreateTransactionalBatch(new PartitionKey(partitionKeyValue));
				foreach (var id in chunk)
					batch.DeleteItem(id);

				using var response = await batch.ExecuteAsync(cancellationToken);
				if (!response.IsSuccessStatusCode)
					throw new InvalidOperationException($"Cosmos transactional delete failed with status {(int)response.StatusCode}.");
			}
		}


		public async Task SaveData<T>(string comsosDb, string container, T parameter, CancellationToken cancellationToken = default)
		{
			var containerAccess = _client.GetContainer(comsosDb, container);
			var response = await containerAccess.CreateItemAsync(parameter, cancellationToken: cancellationToken);
			UpdateEtag(parameter, response.ETag);
			
		}

        public async Task UpsertItem<T>(string cosmosDb, string container, T parameter, CancellationToken cancellationToken = default)
        {
            var containerAccess = _client.GetContainer(cosmosDb, container);
            await UpsertVersionedItem(containerAccess, parameter, partitionKey: null, cancellationToken);
        }

		public async Task UpsertItem<T>(string cosmosDb, string container, T parameter, string partitionKeyValue, CancellationToken cancellationToken = default)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			await UpsertVersionedItem(containerAccess, parameter, new PartitionKey(partitionKeyValue), cancellationToken);
		}

		private static async Task UpsertVersionedItem<T>(
			Container container,
			T parameter,
			PartitionKey? partitionKey,
			CancellationToken cancellationToken)
		{
			var requestOptions = parameter is IVersionedQueryItem { ETag: { Length: > 0 } etag }
				? new ItemRequestOptions { IfMatchEtag = etag }
				: null;

			try
			{
				var response = await container.UpsertItemAsync(parameter, partitionKey, requestOptions, cancellationToken);
				UpdateEtag(parameter, response.ETag);
			}
			catch (CosmosException exception) when (exception.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
			{
				throw new OptimisticConcurrencyException(
					"The document was changed by another request. Reload it and try again.",
					exception);
			}
		}

		private static void UpdateEtag<T>(T parameter, string etag)
		{
			if (parameter is IVersionedQueryItem versioned)
				versioned.ETag = etag;
		}

		private async Task<List<T>> QueryItems<T>(
			string cosmosDb,
			string container,
			QueryDefinition query,
			QueryRequestOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			var containerAccess = _client.GetContainer(cosmosDb, container);
			var iterator = containerAccess.GetItemQueryIterator<T>(query, requestOptions: options);
			var results = new List<T>();

			while (iterator.HasMoreResults)
			{
				foreach (var item in await iterator.ReadNextAsync(cancellationToken))
				{
					results.Add(item);
				}
			}

			return results;
		}
    }
}
