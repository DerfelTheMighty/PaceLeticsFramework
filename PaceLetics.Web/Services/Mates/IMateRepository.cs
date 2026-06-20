namespace PaceLetics.Web.Services.Mates;

public interface IMateRepository
{
    Task<IReadOnlyList<MateAvailabilityDocument>> GetMateAvailabilitiesForCourseAsync(string courseId);

    Task<IReadOnlyList<MateAvailabilityDocument>> GetMateAvailabilitiesForAthleteAsync(string athleteUserId);

    Task<MateAvailabilityDocument?> GetMateAvailabilityAsync(string courseId, string availabilityId);

    Task UpsertMateAvailabilityAsync(MateAvailabilityDocument availability);

    Task DeleteMateAvailabilityAsync(string courseId, string availabilityId);
}
