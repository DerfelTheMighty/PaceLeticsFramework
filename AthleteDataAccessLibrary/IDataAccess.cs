﻿using CoreLibrary.Models.Contracts;
using Microsoft.Azure.Cosmos;

namespace AthleteDataAccessLibrary
{
	public interface IDataAccess
	{

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="sql"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		Task<List<T>> LoadData<T, U>(string cosmosDb, string container, U parameter);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="cosmosDb"></param>
		/// <param name="container"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		Task<T> LoadItem<T>(string cosmosDb, string container, string id, PartitionKey key) where T : IQueryItem;


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="cosmosDb"></param>
		/// <param name="container"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		Task<T> DeleteItem<T, U>(string cosmosDb, string container, U parameter);


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