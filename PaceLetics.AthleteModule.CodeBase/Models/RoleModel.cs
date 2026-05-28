namespace PaceLetics.AthleteModule.CodeBase.Models
{
    public class RoleModel
    {
        public const string Athlete = "Athlete";
        public const string Trainer = "Trainer";

        public List<string> AssignedRoles { get; set; } = new() { Athlete };
    }
}
