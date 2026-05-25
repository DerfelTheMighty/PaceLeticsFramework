namespace PaceLetics.WorkoutModule.CodeBase.Interfaces
{
    public interface IWorkoutService
    {
        IWorkout GetWorkout(string id);
        void SetActiveWorkout(string id, int sets, int rounds);
        IWorkout? GetActiveWorkout();
    }
}
