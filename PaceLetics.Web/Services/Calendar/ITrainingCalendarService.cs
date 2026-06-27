namespace PaceLetics.Web.Services.Calendar;

public interface ITrainingCalendarService
{
    Task<IReadOnlyList<TrainingCalendarItem>> GetCalendarItemsForAthleteAsync(string athleteUserId);
}
