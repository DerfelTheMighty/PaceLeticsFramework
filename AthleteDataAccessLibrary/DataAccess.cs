using CoreLibrary.Models.Athlet;
using Dapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;


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


		/// <summary>
		/// Returns all available items from the container
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="cosmosDb"></param>
		/// <param name="container"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
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




		/// <summary>
		/// Save an item to the container
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="comsosDb"></param>
		/// <param name="container"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public async Task SaveData<T>(string comsosDb, string container, T parameter)
		{
			var containerAccess = _client.GetContainer(comsosDb, container);
			await containerAccess.CreateItemAsync(parameter);
			
		}

	}
}
