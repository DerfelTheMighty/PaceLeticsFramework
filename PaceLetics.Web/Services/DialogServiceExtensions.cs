using MudBlazor;

namespace PaceLetics.Web.Services;

public static class DialogServiceExtensions
{
    public static Task<bool?> ShowInformationAsync(
        this IDialogService dialogs,
        string title,
        string message,
        string? icon = null)
    {
        ArgumentNullException.ThrowIfNull(dialogs);

        return dialogs.ShowMessageBoxAsync(
            title,
            message,
            yesText: "OK",
            options: new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            });
    }
}
