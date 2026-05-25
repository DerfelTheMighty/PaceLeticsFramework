using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
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
