using System.ComponentModel.DataAnnotations;


namespace PaceLetics.WorkoutModule.CodeBase.Enums
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
