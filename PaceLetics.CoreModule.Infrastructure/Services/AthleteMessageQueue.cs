using PaceLetics.CoreModule.Infrastructure.Models;

namespace PaceLetics.CoreModule.Infrastructure.Services;

public sealed class AthleteMessageQueue
{
    private readonly List<QueuedAthleteMessage> _messages = new();

    public void Enqueue(AthleteMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _messages.Add(new QueuedAthleteMessage(message, _messages.Count));
    }

    public IReadOnlyList<AthleteMessage> Drain()
    {
        return _messages
            .OrderByDescending(item => item.Message.Priority)
            .ThenBy(item => item.Sequence)
            .Select(item => item.Message)
            .ToList()
            .AsReadOnly();
    }

    private sealed record QueuedAthleteMessage(AthleteMessage Message, int Sequence);
}
