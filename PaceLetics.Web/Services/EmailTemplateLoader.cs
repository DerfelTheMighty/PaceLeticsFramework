using System.Reflection;
using System.IO;

namespace PaceLetics.Web.Services
{
    public static class EmailTemplateLoader
    {
        /// <summary>
        /// Lädt ein eingebettetes HTML-Template aus dem Assembly-Manifest.
        /// </summary>
        /// <param name="filename">Dateiname inkl. Endung, z. B. "ConfirmationEmailTemplate.html"</param>
        /// <returns>HTML-Inhalt als String</returns>
        public static string LoadTemplate(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"PaceLetics.Web.Resources.EmailTemplates.{filename}";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Template '{resourceName}' nicht gefunden.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
