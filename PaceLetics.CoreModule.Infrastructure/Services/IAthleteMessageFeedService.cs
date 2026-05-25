using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Services;

public interface IAthleteMessageFeedService
{
    IReadOnlyList<AthleteMessage> Build(AthleteMessageContext context);
}
