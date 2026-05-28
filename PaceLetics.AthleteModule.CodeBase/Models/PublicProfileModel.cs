namespace PaceLetics.AthleteModule.CodeBase.Models
{
    public class PublicProfileModel
    {
        public string PublicUserName { get; set; } = "NA";

        public string NormalizedPublicUserName { get; set; } = "NA";

        public string? ProfileImageUrl { get; set; }

        public bool IsProfileVisible { get; set; }

        public string PublicRole { get; set; } = RoleModel.Athlete;
    }
}
