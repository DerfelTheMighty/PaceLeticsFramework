using System.Globalization;
using MudBlazor;

namespace PaceLetics.Web.Services;

public static class UiConfirmation
{
    public static async Task<bool> AskAsync(IDialogService dialogs, string message, bool destructive = true)
    {
        var german = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de";
        var result = await dialogs.ShowMessageBoxAsync(
            german ? "Bitte bestätigen" : "Please confirm",
            message,
            yesText: destructive ? (german ? "Löschen" : "Delete") : (german ? "Bestätigen" : "Confirm"),
            cancelText: german ? "Abbrechen" : "Cancel",
            options: new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.ExtraSmall
            });

        return result == true;
    }
}
