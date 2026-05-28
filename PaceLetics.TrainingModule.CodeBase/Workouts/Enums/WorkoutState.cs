using System.ComponentModel.DataAnnotations;


namespace PaceLetics.TrainingModule.CodeBase.Workouts.Enums
{
    public enum WorkoutState
    {
        [Display(Name = "Pause")]
        Pause,

        [Display(Name = "Aktiv")]
        Running,

        [Display(Name = "Stop")]
        Stop,

        [Display(Name = "Fertig")]
        Finished


    }
}
