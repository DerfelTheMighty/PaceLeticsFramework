using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PaceLetics.Web.Shared;

public sealed class LoggingErrorBoundary : ErrorBoundary
{
    [Inject] private ILogger<LoggingErrorBoundary> Logger { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(exception, "An unhandled Blazor component error occurred.");
        return Task.CompletedTask;
    }
}
