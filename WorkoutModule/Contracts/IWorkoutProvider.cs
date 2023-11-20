
using WorkoutModule.Logic;
using WorkoutModule.Models;

namespace WorkoutModule.Contracts
{
	public interface IWorkoutProvider
	{
		WorkoutPreview GetWorkoutPreview(string id);
		Workout GetWorkout(string id);
		List<string> GetWorkoutIds();

	}
}
