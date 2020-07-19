﻿using System.Text.Json.Serialization;

namespace HabboGallery.Desktop.Web.Json
{
    public class BatchRequest
    {
        [JsonPropertyName("login_key")]
        public string LoginKey { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("photo_ids")]
        public int[] PhotoIds { get; set; }

        public BatchRequest(string loginKey, string countryCode, int[] photoIds)
        {
            LoginKey = loginKey;
            CountryCode = countryCode;
            PhotoIds = photoIds;
        }
    }
}
