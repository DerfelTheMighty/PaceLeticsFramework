
namespace PaceLetics.CoreModule.Infrastructure.Constants
{
	public static class RaceDistances
	{
		public const long D1200Meters = 1200;
		public const long D3000Meters = 3000;
		public const long D3600Meters = 3600;

		public static Dictionary<string, long> Dict { get; private set; } = new Dictionary<string, long>()
		{ {"1 km",1000},
		  {"1200 m", D1200Meters },
		  {"3 km", D3000Meters },
		  {"3600 m", D3600Meters },
		  {"5 km", 5000 },
		  {"10 km", 10000 },
		  {"15 km", 15000 },
		  {"Halbmarathon", 21097 },
		  {"Marathon", 42195 }
		};
	}
}
