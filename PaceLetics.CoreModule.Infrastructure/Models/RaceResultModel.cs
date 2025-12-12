
using PaceLetics.CoreModule.Infrastructure.Constants;

namespace PaceLetics.CoreModule.Infrastructure.Models
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
				"Time: " + Time.ToString(@"hh\:mm\:ss");
			return result;
		}

	}


    public class RaceResultModelFactory
    {
        public List<RaceResultModel> CreateRaceResults()
        {
            List<RaceResultModel> models = new List<RaceResultModel>();
            models.Add(new RaceResultModel()
            {
                Id = "Würstchenbudenlauf",
                Date = DateTime.Now,
                Type = RaceKeys.D10k,
                DistanceM = RaceDistances.Dict[RaceKeys.D10k],
                Time = new TimeSpan(0, 39, 44)
            });
            models.Add(new RaceResultModel()
            {
                Id = "Möllner Citylauf",
                Date = DateTime.Now,
                Type = RaceKeys.D15k,
                DistanceM = RaceDistances.Dict[RaceKeys.D15k],
                Time = new TimeSpan(0, 59, 32)
            });
            models.Add(new RaceResultModel()
            {
                Id = "Testlauf 3k",
                Date = DateTime.Now,
                Type = RaceKeys.D3k,
                DistanceM = RaceDistances.Dict[RaceKeys.D3k],
                Time = new TimeSpan(0, 10, 45)
            });
            return models;
        }
    }

}
