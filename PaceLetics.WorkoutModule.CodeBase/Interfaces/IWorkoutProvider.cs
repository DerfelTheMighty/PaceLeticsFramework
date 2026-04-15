

using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
	public interface IWorkoutProvider
	{
		WorkoutPreview GetWorkoutPreview(string id);
		IWorkout GetWorkout(string id);
		List<string> GetWorkoutIds();
     // Returns the distinct base workout names (e.g. "Stabi Handout")
		List<string> GetBaseWorkoutNames();
		// Returns all workout ids (variants) that belong to the given base workout name
		List<string> GetWorkoutIdsByName(string name);
		void SetActiveWorkout(string id, int sets, int rounds);
		IWorkout GetActiveWorkout();

	}
}
