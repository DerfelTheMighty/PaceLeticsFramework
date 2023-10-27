
namespace CoreLibrary.Constants
{
	public static class RaceDistances
	{
		public static Dictionary<string, long> Dict { get; private set; } = new Dictionary<string, long>()
		{ {"1 km",1000},
		  {"3 km", 3000 },
		  {"5 km", 5000 },
		  {"10 km", 10000 },
		  {"15 km", 15000 },
		  {"Halbmarathon", 21097 },
		  {"Marathon", 42195 }
		};
	}
}
