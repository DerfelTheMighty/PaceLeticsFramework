using PaceLetics.CoreModule.Infrastructure.Constants;

using PaceLetics.CoreModule.Infrastructure.Enums;

namespace PaceLetics.CoreModule.Infrastructure.Models
{
	public class PaceModel
	{
		public double Vdot { get; set; }

		/// <summary>
		/// Per km timespan for easy running
		/// </summary>
		public TimeSpan Easy { get; set; }

		/// <summary>
		/// Per km timespan for threshold pace running
		/// </summary>
		public TimeSpan Threshold { get; set; }

		/// <summary>
		/// Per km timespan for intervall pace running
		/// </summary>
		public TimeSpan Intervall { get; set; }

		/// <summary>
		/// Per km timespan for repetition pace running
		/// </summary>
		public TimeSpan Repetition { get; set; }

		/// <summary>
		/// Per km timespan for marathon pace running
		/// </summary>
		public TimeSpan Marathon { get; set; }

		/// <summary>
		/// overrides to string method and returns a formatted string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string result = "Vdot: " + Vdot.ToString("0.#") + "| " +
							"Easy: " + Easy.ToString(@"mm\:ss") + "| " +
							"Marahon: " + Marathon.ToString(@"mm\:ss") + "| " +
							"Threshold: " + Threshold.ToString(@"mm\:ss") + "| " +
							"Intervall: " + Intervall.ToString(@"mm\:ss") + "| " +
							"Repetition: " + Repetition.ToString(@"mm\:ss");

			return result;
		}

		public TimeSpan GetPace(Pace pace)
		{
			return pace switch
			{
				Pace.Easy => Easy,
				Pace.Marathon => Marathon,
				Pace.Threshold => Threshold,
				Pace.Intervall => Intervall,
				Pace.Repetition => Repetition,
				_ => throw new ArgumentOutOfRangeException(nameof(pace), pace, null)
			};
		}

		public TimeSpan GetPace(string paceKey) 
		{
			return TryGetPace(paceKey, out var pace)
				? pace
				: throw new ArgumentException($"Unknown pace key '{paceKey}'.", nameof(paceKey));
		}

		public bool TryGetPace(string? paceKey, out TimeSpan pace)
		{
			pace = paceKey switch
			{
				PaceKeys.Easy => Easy,
				PaceKeys.Marathon => Marathon,
				PaceKeys.Threshold => Threshold,
				PaceKeys.Intervall => Intervall,
				PaceKeys.Repetition => Repetition,
				PaceKeys.Recovery => Easy.Add(TimeSpan.FromSeconds(30)),
				_ => default
			};

			return paceKey is PaceKeys.Easy
				or PaceKeys.Marathon
				or PaceKeys.Threshold
				or PaceKeys.Intervall
				or PaceKeys.Repetition
				or PaceKeys.Recovery;
		}

		/// <summary>
		/// Returns a slowed copy of the current pace model
		/// </summary>
		/// <param name="factor">value between 0 and 1</param>
		/// <returns></returns>
		public PaceModel Reduce(double factor) 
		{
			PaceModel pm = new PaceModel();
			pm.Easy = new TimeSpan(0, 0, (int)(this.Easy.TotalSeconds/(factor*factor)));
            pm.Marathon = new TimeSpan(0, 0, (int)(this.Marathon.TotalSeconds / (factor) ));
            pm.Threshold = new TimeSpan(0, 0, (int)(this.Threshold.TotalSeconds / factor));
            pm.Intervall = new TimeSpan(0, 0, (int)(this.Intervall.TotalSeconds / factor));
            pm.Repetition = new TimeSpan(0, 0, (int)(this.Repetition.TotalSeconds / factor));
			return pm;
        }

	}
}
