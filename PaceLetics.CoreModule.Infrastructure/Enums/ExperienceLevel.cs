using System.ComponentModel;

namespace PaceLetics.CoreModule.Infrastructure.Enums
{
    [DefaultValue(ExperienceLevel.None)]
    public enum ExperienceLevel
	{
        None,
        Novice,
		Intermediate,
		Expert
	}
}
