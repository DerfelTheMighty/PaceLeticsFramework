using PaceLetics.TrainingModule.CodeBase.Workouts.Models;

namespace PaceLetics.Web.Services.Workouts;

public interface IWorkoutCatalogStore
{
    Task<WorkoutCatalogDocument> LoadOrSeedAsync(WorkoutCatalogDocument seedCatalog);
    Task SaveAsync(WorkoutCatalogDocument catalog);
}
