using System;
using System.IO;
using Newtonsoft.Json;

namespace FLyC0De.Core.Configuration
{
    /// <summary>
    /// Manages loading and saving application configuration.
    /// </summary>
    public class ConfigurationManager
    {
        private readonly string _configPath;
        private readonly JsonSerializerSettings _jsonSettings;

        public AppConfiguration Config { get; private set; }

        public ConfigurationManager()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FLyC0De Suite");
            
            Directory.CreateDirectory(appDataPath);
            _configPath = Path.Combine(appDataPath, "config.json");

            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            Config = new AppConfiguration();
        }

        /// <summary>
        /// Loads configuration from disk.
        /// </summary>
        public bool Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    Config = JsonConvert.DeserializeObject<AppConfiguration>(json, _jsonSettings);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
            }

            Config = new AppConfiguration();
            return false;
        }

        /// <summary>
        /// Saves configuration to disk.
        /// </summary>
        public bool Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Config, _jsonSettings);
                File.WriteAllText(_configPath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        public string ConfigPath => _configPath;
    }
}
