using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Services;

public sealed class AthleteMessageFeedService : IAthleteMessageFeedService
{
    private readonly IEnumerable<IAthleteMessageProvider> _providers;

    public AthleteMessageFeedService(IEnumerable<IAthleteMessageProvider> providers)
    {
        _providers = providers;
    }

    public IReadOnlyList<AthleteMessage> Build(AthleteMessageContext context)
    {
        var queue = new AthleteMessageQueue();

        foreach (var provider in _providers)
        {
            provider.Enqueue(context, queue);
        }

        return queue.Drain();
    }
}
