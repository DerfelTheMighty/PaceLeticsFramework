﻿using System.ComponentModel.DataAnnotations;


namespace PaceLetics.WorkoutModule.CodeBase.Enums
{
    public enum Level
    {
        [Display(Name = "NA")]
        None,

        [Display(Name = "Leicht")]
        Easy,

        [Display(Name = "Fortgeschritten")]
        Moderate,

        [Display(Name = "Schwer")]
        Advanced,

        [Display(Name = "Episch")]
        Epic
    }
}
