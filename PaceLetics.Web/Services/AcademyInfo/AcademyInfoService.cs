namespace PaceLetics.Web.Services.AcademyInfo;

public sealed class AcademyInfoService : IDisposable
{
    private static readonly AcademyInfoTip[] TipRepertoire =
    [
        new("Pacegesteuertes Training", "Pace ist aeussere Last; Puls zeigt innere Beanspruchung. Beides erzaehlt etwas anderes.", "/Athletes/academy/pace-controlled-training"),
        new("Pacegesteuertes Training", "Steigere lieber in Bloecken als sprunghaft. So koennen langsame Strukturen besser mitziehen.", "/Athletes/academy/pace-controlled-training"),
        new("Pacegesteuertes Training", "Schnelle Paces sind kein Problem, wenn sie dosiert kommen und nicht jede Einheit dominieren.", "/Athletes/academy/pace-controlled-training"),
        new("Sehnenadaptation", "Sehnen denken in Wochen und Monaten. Kleine Progressionen sind oft die kluegere Abkuerzung.", "/Athletes/academy/tendon-adaptation"),
        new("Sehnenadaptation", "Fuer Sehnen zaehlt kontrollierte Spannung mehr als Hektik. Langsam, schwer, sauber bleibt stark.", "/Athletes/academy/tendon-adaptation"),
        new("Sehnenadaptation", "Wenn du dich aerob schnell fitter fuehlst: Gib Sehnen und Knochen trotzdem Zeit zum Nachziehen.", "/Athletes/academy/tendon-adaptation"),
        new("Mentale Ressource", "Ein lockerer Lauf darf einfach den Tag sortieren. Nicht jede Einheit braucht Heldentum.", "/Athletes/academy/mental-resource-running"),
        new("Mentale Ressource", "Training ist auch dann gelungen, wenn es Laufen morgen wahrscheinlicher macht.", "/Athletes/academy/mental-resource-running"),
        new("Mentale Ressource", "Gute Routinen schuetzen die Ressource Laufen: verlaesslich, dosiert und alltagstauglich.", "/Athletes/academy/mental-resource-running"),
        new("Laufanalyse", "Technikveraenderung wirkt besser klein dosiert. Ein Fokus pro Lauf reicht meistens.", "/Athletes/academy/evidence-based-running-analysis"),
        new("Laufanalyse", "Analyse ist kein Urteil. Sie zeigt, welcher kleine Hebel als naechstes sinnvoll ist.", "/Athletes/academy/evidence-based-running-analysis"),
        new("Verletzte Athleten", "Beschwerden sind ein Signal fuer Anpassung, nicht fuer Panik. Reduziere zuerst Druck, Tempo oder Umfang.", "/Athletes/academy/injured-athletes")
    ];

    private readonly TimeProvider _timeProvider;
    private readonly Random _random;
    private readonly TimeSpan _displayDuration;
    private readonly TimeSpan _minimumGap;
    private readonly int _navigationCadence;
    private readonly object _syncRoot = new();
    private DateTimeOffset _lastShownAt = DateTimeOffset.MinValue;
    private CancellationTokenSource? _hideCancellation;
    private int _navigationCount;
    private int? _lastTipIndex;
    private bool _disposed;

    public AcademyInfoService()
        : this(
            TimeProvider.System,
            Random.Shared,
            TimeSpan.FromMilliseconds(4300),
            navigationCadence: 3,
            minimumGap: TimeSpan.FromSeconds(14))
    {
    }

    public AcademyInfoService(
        TimeProvider timeProvider,
        Random random,
        TimeSpan displayDuration,
        int navigationCadence,
        TimeSpan minimumGap)
    {
        _timeProvider = timeProvider;
        _random = random;
        _displayDuration = displayDuration;
        _navigationCadence = Math.Max(1, navigationCadence);
        _minimumGap = minimumGap < TimeSpan.Zero ? TimeSpan.Zero : minimumGap;
    }

    public event Action? Changed;

    public bool IsVisible { get; private set; }
    public AcademyInfoTip? CurrentTip { get; private set; }
    public IReadOnlyList<AcademyInfoTip> Tips => TipRepertoire;

    public async Task TrackNavigationAsync(string absoluteUri)
    {
        if (_disposed || !ShouldConsiderRoute(absoluteUri))
            return;

        if (!TryShowForNavigation())
            return;

        try
        {
            var cancellation = _hideCancellation?.Token ?? CancellationToken.None;
            await Task.Delay(_displayDuration, _timeProvider, cancellation);
            Hide();
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Hide()
    {
        if (_disposed)
            return;

        lock (_syncRoot)
        {
            if (!IsVisible && CurrentTip is null)
                return;

            _hideCancellation?.Cancel();
            _hideCancellation?.Dispose();
            _hideCancellation = null;
            IsVisible = false;
            CurrentTip = null;
        }

        NotifyChanged();
    }

    private bool TryShowForNavigation()
    {
        lock (_syncRoot)
        {
            _navigationCount++;

            if (IsVisible || _navigationCount % _navigationCadence != 0)
                return false;

            var now = _timeProvider.GetUtcNow();
            if (now - _lastShownAt < _minimumGap)
                return false;

            CurrentTip = SelectTip();
            IsVisible = true;
            _lastShownAt = now;
            _hideCancellation?.Cancel();
            _hideCancellation?.Dispose();
            _hideCancellation = new CancellationTokenSource();
        }

        NotifyChanged();
        return true;
    }

    private AcademyInfoTip SelectTip()
    {
        if (TipRepertoire.Length == 1)
        {
            _lastTipIndex = 0;
            return TipRepertoire[0];
        }

        var index = _random.Next(TipRepertoire.Length - 1);
        if (_lastTipIndex is int lastIndex && index >= lastIndex)
            index++;

        _lastTipIndex = index;
        return TipRepertoire[index];
    }

    private static bool ShouldConsiderRoute(string absoluteUri)
    {
        if (!Uri.TryCreate(absoluteUri, UriKind.Absolute, out var uri))
            return true;

        var path = uri.AbsolutePath.Trim('/');
        return !path.StartsWith("Identity", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith("Account", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith("_blazor", StringComparison.OrdinalIgnoreCase);
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }

    public void Dispose()
    {
        _disposed = true;
        _hideCancellation?.Cancel();
        _hideCancellation?.Dispose();
    }
}
