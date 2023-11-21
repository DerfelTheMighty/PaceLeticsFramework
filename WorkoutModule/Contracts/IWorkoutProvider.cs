using WorkoutModule.Logic;
using WorkoutModule.Models;

namespace WorkoutModule.Contracts
{
	public interface IWorkoutProvider
	{
		WorkoutPreview GetWorkoutPreview(string id);
		IWorkout GetWorkout(string id);
		List<string> GetWorkoutIds();
		void SetActiveWorkout(string id);
		IWorkout GetActiveWorkout();

	}
}
