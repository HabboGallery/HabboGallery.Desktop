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

        public int UserInterceptionTriggerCountThreshold { get; set; } = 20;
        public IList<string> UserInterceptionTriggers { get; set; } = new[] { "accountId", "buildersClubMember", "creationTime", "country", "email", "emailVerified", "figureString", "force", "habboClubMember", "identityId", "identityType", "identityVerified", "loginLogId", "name", "partner", "sessionLogId", "traits", "trusted", "uniqueId", "motto", "lastWebAccess" };
        public IList<string> WASMInterceptionTriggers { get; set; } = new[] { "habbo2020-global-prod.data.unityweb", "habbo2020-global-prod.wasm.code.unityweb", "habbo2020-global-prod.wasm.framework.unityweb" };

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
