namespace PaceLetics.Web.Services.Mates;

public interface IMateService
{
    Task<MateOverview> GetOverviewAsync(string? athleteUserId);

    Task<MateAvailabilityDocument> ShareSessionAsync(string? athleteUserId, MateShareRequest request);

    Task RemoveAvailabilityAsync(string? athleteUserId, string availabilityId);
}
