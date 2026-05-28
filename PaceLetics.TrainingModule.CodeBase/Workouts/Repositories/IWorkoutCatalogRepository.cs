using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.TrainingModule.CodeBase.Workouts.Repositories
{
    public interface IWorkoutCatalogRepository
    {
        WorkoutCatalogDocument Load();
    }
}
