using System.Text;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace PaceLetics.RunningModule.CodeBase.Models
{
    /// <summary>
    /// Lädt mehrere Trainingspläne aus einem Verzeichnis. Jede JSON-Datei wird als eigener Plan behandelt.
    /// Dateiname (ohne Extension) wird als Plan.Id und -Name verwendet (einfach lesbar formatiert).
    /// Jede Datei muss ein JSON-Array / Objekt im bestehenden Format für Sessions enthalten (wie intervalls.json).
    /// </summary>
    public static class TrainingPlanProvider
    {
        /// <summary>
        /// Lädt alle Pläne aus dem angegebenen Verzeichnis (lokaler Pfad). Gibt leere Liste zurück, wenn Verzeichnis fehlt oder keine Dateien.
        /// </summary>
        public static IReadOnlyList<TrainingPlan> LoadFromDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                return Array.Empty<TrainingPlan>();

            var files = Directory.EnumerateFiles(directoryPath, "*.json");
            var plans = new List<TrainingPlan>();

            foreach (var file in files)
            {
                try
                {
                    var sessions = RunningSessionFactory.Load(file);
                    var id = Path.GetFileNameWithoutExtension(file) ?? Guid.NewGuid().ToString();
                    var name = ToReadableName(id);
                    plans.Add(new TrainingPlan(id, name, sessions));
                }
                catch
                {
                    // Ungültige/fehlerhafte Datei überspringen (kein Crash)
                }
            }

            return plans;
        }

        private static string ToReadableName(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return id;
            var s = id.Replace('-', ' ').Replace('_', ' ');
            // Capitalize first letter
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
