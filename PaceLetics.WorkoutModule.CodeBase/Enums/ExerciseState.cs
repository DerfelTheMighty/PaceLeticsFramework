using System.ComponentModel.DataAnnotations;

namespace PaceLetics.WorkoutModule.CodeBase.Enums
{

    public enum ExerciseState
    {

        [Display(Name = "Pause")]
        Pause,
        
        [Display(Name = "Seitenwechsel")]
        Switch,

        [Display(Name = "Aktiv")]
        Running,

        [Display(Name = "Stop")]
        Stop

    }

}
