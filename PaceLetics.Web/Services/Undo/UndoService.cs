namespace PaceLetics.Web.Services.Undo;

public sealed record UndoOffer(Guid Id, string Message, string ActionLabel, Func<Task> Undo);

public sealed class UndoService : IDisposable
{
    private CancellationTokenSource? _expiry;
    public event Action? Changed;
    public UndoOffer? Current { get; private set; }

    public void Offer(string message, string actionLabel, Func<Task> undo, TimeSpan? duration = null)
    {
        _expiry?.Cancel();
        _expiry?.Dispose();
        _expiry = new CancellationTokenSource();
        Current = new UndoOffer(Guid.NewGuid(), message, actionLabel, undo);
        Changed?.Invoke();
        _ = ExpireAsync(Current.Id, duration ?? TimeSpan.FromSeconds(8), _expiry.Token);
    }

    public async Task UndoAsync()
    {
        var offer = Current;
        if (offer is null) return;
        Clear();
        await offer.Undo();
    }

    public void Clear()
    {
        _expiry?.Cancel();
        _expiry?.Dispose();
        _expiry = null;
        Current = null;
        Changed?.Invoke();
    }

    private async Task ExpireAsync(Guid id, TimeSpan duration, CancellationToken token)
    {
        try
        {
            await Task.Delay(duration, token);
            if (Current?.Id == id) Clear();
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _expiry?.Cancel();
        _expiry?.Dispose();
    }
}
