
namespace PaceLetics.VdotModule.CodeBase.Models
{
	/// <summary>
	/// Reference performance consists of a distance, a reference time and a target pace
	/// </summary>
	public class RaceResultModel
	{
		/// <summary>
		/// Id or name of the event
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Race Type
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Reference Distance
		/// </summary>
		public long DistanceM { get; set; }

		/// <summary>
		/// Race date
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// Time in Seconds
		/// </summary>
		public TimeSpan Time { get; set; }

		
        public RaceResultModel()
        {
            Id = string.Empty;
            Type = string.Empty;
            Date = DateTime.Now;
            DistanceM = 0;
            Time = new TimeSpan(0, 0, 0);
        }

        /// <summary>
        /// overrides to string method and returns a formatted string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
		{
			string result =
				"ID: " + Id + " | " +
				"Name: " + Type + "  | " +
				"Date: " + Date.ToString("yyyyMMdd")  +
				"Distance: " + DistanceM + " m | " +
				"Time: " + Time.ToString(@"hh\\:mm\\:ss");
			return result;
		}

	}
}
