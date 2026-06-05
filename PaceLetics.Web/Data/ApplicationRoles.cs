using PaceLetics.AthleteModule.CodeBase.Models;

namespace PaceLetics.Web.Data
{
    public static class ApplicationRoles
    {
        public const string Athlete = RoleModel.Athlete;
        public const string Trainer = RoleModel.Trainer;

        public static readonly string[] All = { Athlete, Trainer };
    }
}
