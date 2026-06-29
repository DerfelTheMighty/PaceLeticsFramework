using System.Net.Http.Json;

namespace PaceLetics.Web.Services.SignalBot;

public sealed class SignalCliRestApiMessageClient : ISignalMessageClient
{
    private readonly HttpClient _httpClient;

    public SignalCliRestApiMessageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendAsync(
        string senderNumber,
        IReadOnlyList<string> recipients,
        string message,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "v2/send",
            new
            {
                message,
                number = senderNumber,
                recipients
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
