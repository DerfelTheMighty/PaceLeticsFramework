using System.ComponentModel.DataAnnotations;

namespace WorkoutModule.Enums
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
