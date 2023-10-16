namespace CoreLibrary.Models.Pace
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

	}
}
