namespace PaceLetics.Web.Services.Loading;

public sealed class LoadingStateService
{
    private readonly Stack<LoadingState> _states = new();

    public event Action? Changed;

    public bool IsLoading => _states.Count > 0;
    public string Label => _states.TryPeek(out var state) ? state.Label : string.Empty;

    public IDisposable Show(string label)
    {
        var state = new LoadingState(string.IsNullOrWhiteSpace(label) ? "Loading..." : label);
        _states.Push(state);
        NotifyChanged();

        return new LoadingScope(this, state);
    }

    public async Task RunAsync(string label, Func<Task> operation)
    {
        using var scope = Show(label);
        await operation();
    }

    public async Task<T> RunAsync<T>(string label, Func<Task<T>> operation)
    {
        using var scope = Show(label);
        return await operation();
    }

    private void Hide(LoadingState state)
    {
        if (_states.Count == 0)
            return;

        if (_states.Peek() == state)
        {
            _states.Pop();
        }
        else
        {
            var remainingStates = _states
                .Where(existingState => existingState != state)
                .Reverse()
                .ToArray();

            _states.Clear();
            foreach (var remainingState in remainingStates)
                _states.Push(remainingState);
        }

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }

    private sealed record LoadingState(string Label);

    private sealed class LoadingScope : IDisposable
    {
        private readonly LoadingStateService _service;
        private readonly LoadingState _state;
        private bool _disposed;

        public LoadingScope(LoadingStateService service, LoadingState state)
        {
            _service = service;
            _state = state;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _service.Hide(_state);
        }
    }
}
