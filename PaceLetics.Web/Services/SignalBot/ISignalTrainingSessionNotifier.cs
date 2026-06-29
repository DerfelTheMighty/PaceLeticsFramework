namespace PaceLetics.Web.Services.SignalBot;

public interface ISignalTrainingSessionNotifier
{
    Task<SignalBotPostResult> PostCurrentTrainingSessionsAsync(CancellationToken cancellationToken);
}
