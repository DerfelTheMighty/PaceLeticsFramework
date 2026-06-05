namespace PaceLetics.Web.Localization;

public static class SupportedCultures
{
    public static readonly IReadOnlyList<SupportedCulture> All =
    [
        new("de", "German", "Deutsch"),
        new("en", "English", "English"),
        new("tr", "Turkish", "Türkçe"),
        new("da", "Danish", "Dansk"),
        new("ar", "Arabic", "العربية"),
        new("ru", "Russian", "Русский"),
        new("fr", "French", "Français"),
        new("zh", "Chinese", "中文"),
        new("es", "Spanish", "Español"),
        new("fa", "Persian", "فارسی")
    ];

    public static string[] Codes { get; } = All.Select(culture => culture.Code).ToArray();
}
