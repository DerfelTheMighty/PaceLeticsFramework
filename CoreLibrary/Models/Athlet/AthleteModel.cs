using CoreLibrary.Enums;
using CoreLibrary.Models.Contracts;
using CoreLibrary.Models.Pace;
using CoreLibrary.Models.Race;

namespace CoreLibrary.Models.Athlet
{
    public class AthleteModel : IQueryItem
    {

        /// <summary>
        /// Unique id for anonymization
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Name of the athlete
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// used as partition key
        /// </summary>
        public string? AthleteId { get; set; }
        /// <summary>
        /// Current skill level
        /// </summary>
        public ExperienceLevel Level { get; set; }

        /// <summary>
        /// Vdot value
        /// </summary>
        public double Vdot { get; set; }

        /// <summary>
        /// list of race results
        /// </summary>
        public List<RaceResultModel>? RaceResults { get; set; }

        /// <summary>
        /// Active pacemodel
        /// </summary>
        public PaceModel? PaceModel { get; set; }


    }
}
