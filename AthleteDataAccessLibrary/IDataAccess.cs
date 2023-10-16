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
		/// <param name="cosmosDb"></param>
		/// <param name="containerId"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		Task SaveData<T>(string cosmosDb, string containerId, T parameter);
	}
}