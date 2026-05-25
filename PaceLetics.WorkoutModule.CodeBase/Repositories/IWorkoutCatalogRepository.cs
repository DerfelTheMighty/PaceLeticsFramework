using PaceLetics.WorkoutModule.CodeBase.Models;

namespace PaceLetics.WorkoutModule.CodeBase.Repositories
{
    public interface IWorkoutCatalogRepository
    {
        WorkoutCatalogDocument Load();
    }
}
