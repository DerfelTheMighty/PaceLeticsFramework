using System.Globalization;

namespace PaceLetics.Web.Services;

public static class UiErrorMessages
{
    public static string From(Exception exception)
    {
        var german = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de";

        return exception switch
        {
            TimeoutException => german
                ? "Die Anfrage dauert länger als erwartet. Bitte versuche es erneut."
                : "The request is taking longer than expected. Please try again.",
            HttpRequestException => german
                ? "Die Verbindung zum Dienst ist fehlgeschlagen. Prüfe deine Verbindung und versuche es erneut."
                : "The service could not be reached. Check your connection and try again.",
            UnauthorizedAccessException => german
                ? "Du hast für diese Aktion keine Berechtigung."
                : "You do not have permission to perform this action.",
            _ => german
                ? "Die Aktion konnte nicht abgeschlossen werden. Bitte versuche es erneut."
                : "The action could not be completed. Please try again."
        };
    }
}
