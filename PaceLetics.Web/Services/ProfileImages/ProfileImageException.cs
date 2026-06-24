namespace PaceLetics.Web.Services.ProfileImages;

public sealed class ProfileImageException : Exception
{
    public ProfileImageException(ProfileImageError error)
    {
        Error = error;
    }

    public ProfileImageError Error { get; }
}
