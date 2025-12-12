using PaceLetics.CoreModule.Infrastructure.Enums;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models;


namespace PaceLetics.AthleteModule.CodeBase.Models
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
        /// Used as partition key
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
        /// List of race results
        /// </summary>
        public List<RaceResultModel>? RaceResults { get; set; }




        /// <summary>
        /// Active pacemodel
        /// </summary>
        public PaceModel? PaceModel { get; set; }

        public RaceResultModel? ActiveReferenceResult { get; set; }

        public AthleteModel() 
        {
            Id = "NA";
            Name = "NA";
            AthleteId = "NA";
            Level = new ExperienceLevel();
            RaceResults = new List<RaceResultModel>();
            Vdot = 0;
            PaceModel= new PaceModel();
        }

        
    }


    public class AthleteModelFactory
    {
        public List<AthleteModel> CreateAthleteModel()
        {
            List<AthleteModel> list = new List<AthleteModel>();
            list.Add(new AthleteModel()
            {
                Name = "Humbug Hund",
                Id = Guid.NewGuid().ToString(),
                Level = ExperienceLevel.Intermediate,
                Vdot = 45.5
            }
            );
            list.Add(new AthleteModel()
            {
                Name = "Derfel Cadarn",
                Id = Guid.NewGuid().ToString(),
                Level = ExperienceLevel.Novice,
                Vdot = 50.5
            });
            list.Add(new AthleteModel()
            {
                Name = "Jesse Ventura",
                Id = Guid.NewGuid().ToString(),
                Level = ExperienceLevel.Expert,
                Vdot = 56.5
            });
            return list;


        }
    }


}
