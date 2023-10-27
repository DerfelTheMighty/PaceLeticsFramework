using CoreLibrary.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Models.Race
{
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
