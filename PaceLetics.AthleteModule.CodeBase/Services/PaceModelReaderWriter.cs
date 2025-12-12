using System.Text.Json;


namespace PaceLetics.AthleteModule.CodeBase.Services
{
    /// <summary>
    /// Handles the reading and writing of pace model provider
    /// </summary>
    public class PaceModelReaderWriter
    {

        public PaceModelProvider ReadPaceModelFromJson(string path) 
        {
            var options = new JsonSerializerOptions {
                WriteIndented = true
            };

            var jsonstring = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PaceModelProvider>(jsonstring, options)!;
        }

        /// <summary>
        /// Serializes pace model data to the given json file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="provider"></param>
        public void WritePaceModelToJson(string path, PaceModelProvider provider) 
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var jsonString = JsonSerializer.Serialize(provider, options );
            File.WriteAllText(path, jsonString);
        }

    }
}
