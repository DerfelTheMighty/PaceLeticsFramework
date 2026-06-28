using System.Globalization;

namespace PaceLetics.Web.Services.Localization;

public sealed class AppCultureService
{
    private CultureInfo? _culture;

    public CultureInfo CurrentCulture => _culture ?? CultureInfo.CurrentUICulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
    }
}
