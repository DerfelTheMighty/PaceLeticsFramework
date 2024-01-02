

using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
	public interface IWorkoutProvider
	{
		WorkoutPreview GetWorkoutPreview(string id);
		IWorkout GetWorkout(string id);
		List<string> GetWorkoutIds();
		void SetActiveWorkout(string id, int sets, int rounds);
		IWorkout GetActiveWorkout();

	}
}
