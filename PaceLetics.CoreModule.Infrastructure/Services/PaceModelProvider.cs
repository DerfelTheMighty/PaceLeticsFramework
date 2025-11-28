using PaceLetics.CoreModule.Infrastructure.Interfaces;
using PaceLetics.CoreModule.Infrastructure.Models.Vdot;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;


namespace PaceLetics.CoreModule.Infrastructure.Services
{
    /// <summary>
    /// Provides pace models for a given vdot value
    /// </summary>
    public class PaceModelProvider : IPaceModelProvider
    {

        [JsonPropertyName("Registry")]
        public List<PaceModel> Registry { get; set; }

        public PaceModelProvider()
        {
            Registry = new List<PaceModel>();
        }

        [JsonConstructor]
        public PaceModelProvider(List<PaceModel> registry)
        {
            Registry = registry;
        }


        public PaceModel this[double vdot]
        {
            get => Registry[FindClosestIdx(vdot)];
        }


        [JsonIgnore]
        public ICollection<PaceModel> Values => Registry.AsReadOnly<PaceModel>();
        [JsonIgnore]
        public int Count => Registry.Count;


        public void Clear()
        {
            Registry.Clear();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var row in Registry)
            {
                builder.AppendLine(row.ToString());
            }
            return builder.ToString();
        }

        /// <summary>
        /// Creates a demo instance with default values
        /// </summary>
        /// <returns></returns>
        public static PaceModelProvider CreateInstance()
        {
            PaceModelProvider provider = new PaceModelProvider();
            PaceModel model = new PaceModel()
            {
                Vdot = 40,
                Easy = new TimeSpan(0, 6, 54),
                Marathon = new TimeSpan(0, 6, 20),
                Threshold = new TimeSpan(0, 5, 6),
                Intervall = new TimeSpan(0, 4, 40),
                Repetition = new TimeSpan(0, 4, 25)
            };

            provider.Registry.Add(model);

            model = new PaceModel()
            {
                Vdot = 45,
                Easy = new TimeSpan(0, 6, 17),
                Marathon = new TimeSpan(0, 5, 46),
                Threshold = new TimeSpan(0, 4, 38),
                Intervall = new TimeSpan(0, 4, 15),
                Repetition = new TimeSpan(0, 4, 0)
            };

            provider.Registry.Add(model);

            model = new PaceModel()
            {
                Vdot = 50,
                Easy = new TimeSpan(0, 5, 47),
                Marathon = new TimeSpan(0, 5, 18),
                Threshold = new TimeSpan(0, 4, 15),
                Intervall = new TimeSpan(0, 3, 54),
                Repetition = new TimeSpan(0, 3, 39)
            };

            provider.Registry.Add(model);
            return provider;
        }

        /// <summary>
        /// Returns the index of the pace model with the closest vdot value
        /// </summary>
        /// <param name="vdot"></param>
        /// <returns></returns>
        private int FindClosestIdx(double vdot)
        {
            int idx = Registry
                .Select((item, index) => new { Index = index, diff = Math.Abs(item.Vdot - vdot) })
                .OrderBy(item => item.diff)
                .Select(item => item.Index)
                .FirstOrDefault();
            return idx;
        }


    }
}
