using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Services;

public interface IAthleteMessageProvider
{
    void Enqueue(AthleteMessageContext context, AthleteMessageQueue queue);
}
