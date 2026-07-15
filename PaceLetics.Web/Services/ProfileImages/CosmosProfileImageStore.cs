using AthleteDataAccessLibrary;
using AthleteDataAccessLibrary.Contracts;

namespace PaceLetics.Web.Services.ProfileImages;

public sealed class CosmosProfileImageStore : IProfileImageStore
{
    private readonly IDataAccess _data;
    private readonly AthleteDataOptions _options;

    public CosmosProfileImageStore(IDataAccess data, AthleteDataOptions options)
    {
        _data = data;
        _options = options;
    }

    public Task SaveAsync(ProfileImageDocument image, CancellationToken cancellationToken = default)
    {
        return _data.UpsertItem(
            _options.DatabaseName,
            _options.CourseContainerName,
            image,
            ProfileImageDocument.PartitionKeyValue,
            cancellationToken);
    }

    public Task<ProfileImageDocument?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return _data.LoadItem<ProfileImageDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            id,
            ProfileImageDocument.PartitionKeyValue,
            cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _data.DeleteItem<ProfileImageDocument>(
            _options.DatabaseName,
            _options.CourseContainerName,
            id,
            ProfileImageDocument.PartitionKeyValue,
            cancellationToken);
    }
}
