using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces
{
    public interface IWorkoutCatalog
    {
        List<string> GetBaseWorkoutNames();
        WorkoutDefinition GetDefinition(string id);
        WorkoutPreview GetWorkoutPreview(string id);
        List<string> GetWorkoutIds();
        List<string> GetWorkoutIdsByName(string name);
    }
}
