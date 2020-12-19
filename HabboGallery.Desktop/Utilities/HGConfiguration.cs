using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace HabboGallery.Desktop.Utilities
{
    public class HGConfiguration
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        public bool IsFirstConnect { get; set; }
        public IList<string> VisitedHotels { get; set; }

        public void Save(string configFile)
        {
            File.WriteAllText(configFile, JsonSerializer.Serialize(this));
        }

        /// <summary>
        /// Creates a new config file to the specified path. If the config file already exists, the existing config will be returned instead.
        /// </summary>
        public static HGConfiguration Create(string configFile)
        {
            HGConfiguration config;
            if (File.Exists(configFile))
                return JsonSerializer.Deserialize<HGConfiguration>(File.ReadAllBytes(configFile));

            File.WriteAllText(configFile, JsonSerializer.Serialize(config = new HGConfiguration()));
            return config;
        }
    }
}
