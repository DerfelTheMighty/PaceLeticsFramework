using PaceLetics.CoreModule.Infrastructure.Constants;

using PaceLetics.CoreModule.Infrastructure.Converter;
using PaceLetics.CoreModule.Infrastructure.Enums;

namespace PaceLetics.CoreModule.Infrastructure.Models
{
	public class PaceModel
	{
		public double CriticalSpeedMps { get; set; }

		/// <summary>
		/// Retained only so existing athlete documents can be read during migration.
		/// New pace models are based exclusively on Critical Speed.
		/// </summary>
		public double Vdot { get; set; }

		public TimeSpan Recovery { get; set; }

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

		public TimeSpan FastIntervall { get; set; }

		/// <summary>
		/// Retained only for deserializing old Daniels pace models.
		/// </summary>
		public TimeSpan Marathon { get; set; }

		/// <summary>
		/// Retained only for deserializing old Daniels pace models.
		/// </summary>
		public TimeSpan Repetition { get; set; }

		/// <summary>
		/// overrides to string method and returns a formatted string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string result = "Critical Speed: " + PaceFormatting.FormatFromSpeed(CriticalSpeedMps) + "| " +
							"Recovery: " + Recovery.ToString(@"mm\:ss") + "| " +
							"Easy: " + Easy.ToString(@"mm\:ss") + "| " +
							"Threshold: " + Threshold.ToString(@"mm\:ss") + "| " +
							"Intervall: " + Intervall.ToString(@"mm\:ss") + "| " +
							"Fast Intervall: " + FastIntervall.ToString(@"mm\:ss");

			return result;
		}

		public TimeSpan GetPace(Pace pace)
		{
			return pace switch
			{
				Pace.Recovery => ResolveRecovery(),
				Pace.Easy => Easy,
				Pace.Threshold => Threshold,
				Pace.Intervall => Intervall,
				Pace.FastIntervall => ResolveFastIntervall(),
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
			var normalizedPaceKey = PaceKeys.Normalize(paceKey);
			pace = normalizedPaceKey switch
			{
				PaceKeys.Easy => Easy,
				PaceKeys.Threshold => Threshold,
				PaceKeys.Intervall => Intervall,
				PaceKeys.FastIntervall => ResolveFastIntervall(),
				PaceKeys.Recovery => ResolveRecovery(),
				_ => default
			};

			return normalizedPaceKey is PaceKeys.Easy
				or PaceKeys.Threshold
				or PaceKeys.Intervall
				or PaceKeys.FastIntervall
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
			pm.CriticalSpeedMps = CriticalSpeedMps * factor;
			pm.Recovery = new TimeSpan(0, 0, (int)(ResolveRecovery().TotalSeconds / factor));
			pm.Easy = new TimeSpan(0, 0, (int)(this.Easy.TotalSeconds/(factor*factor)));
            pm.Threshold = new TimeSpan(0, 0, (int)(this.Threshold.TotalSeconds / factor));
            pm.Intervall = new TimeSpan(0, 0, (int)(this.Intervall.TotalSeconds / factor));
			pm.FastIntervall = new TimeSpan(0, 0, (int)(ResolveFastIntervall().TotalSeconds / factor));
			return pm;
        }

		private TimeSpan ResolveRecovery() => Recovery != default
			? Recovery
			: Easy == default ? default : Easy.Add(TimeSpan.FromSeconds(30));

		private TimeSpan ResolveFastIntervall() => FastIntervall != default
			? FastIntervall
			: Repetition;

	}
}
