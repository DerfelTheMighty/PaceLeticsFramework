namespace PaceLetics.TrainingModule.CodeBase.Workouts.Interfaces
{
    public interface IWorkoutService
    {
        IWorkout GetWorkout(string id);
        void SetActiveWorkout(string id, int sets, int rounds);
        IWorkout? GetActiveWorkout();
    }
}
