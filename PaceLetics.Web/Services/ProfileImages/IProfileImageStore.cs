namespace PaceLetics.Web.Services.ProfileImages;

public interface IProfileImageStore
{
    Task SaveAsync(ProfileImageDocument image, CancellationToken cancellationToken = default);
    Task<ProfileImageDocument?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
