namespace CoreLibrary.Models.Race
{
	/// <summary>
	/// Reference performance consists of a distance, a reference time and a target pace
	/// </summary>
	public class RaceResultModel
	{
		/// <summary>
		/// Id or name of the event
		/// </summary>
		public string? Id { get; set; }

		/// <summary>
		/// Race Type
		/// </summary>
		public string? Type { get; set; }

		/// <summary>
		/// Reference Distance
		/// </summary>
		public long DistanceM { get; set; }

		/// <summary>
		/// Race date
		/// </summary>
		public DateTime? Date { get; set; }
		/// <summary>
		/// Time in Seconds
		/// </summary>
		public TimeSpan? Time { get; set; }

		/// <summary>
		/// overrides to string method and returns a formatted string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string result =
				"ID: " + Id?.ToString() ?? "NA" + " | " +
				"Name: " + Type?.ToString() ?? "NA" + "  | " +
				"Date: " + Date?.ToString("yyyyMMdd") ?? "NA" +
				"Distance: " + DistanceM.ToString() + " m | " +
				"Time: " + Time?.ToString(@"mm\:ss") ?? "NA" + " hh:mm:ss";
			return result;
		}

	}
}
