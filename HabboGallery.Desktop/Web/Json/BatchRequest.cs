using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabboGallery.Web.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BatchRequest
    {
        [JsonProperty("login_key")]
        public string LoginKey { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("photo_ids")]
        public int[] PhotoIds { get; set; }

        public BatchRequest(string loginKey, string countryCode, int[] photoIds)
        {
            LoginKey = loginKey;
            CountryCode = countryCode;
            PhotoIds = photoIds;
        }
    }
}
