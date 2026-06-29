namespace PaceLetics.Web.Services.SignalBot;

public interface ISignalMessageClient
{
    Task SendAsync(string senderNumber, IReadOnlyList<string> recipients, string message, CancellationToken cancellationToken);
}
