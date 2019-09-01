using Newtonsoft.Json;

namespace HabboGallery.Web.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiResponse<T>
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        public bool Success { get; set; }
    }
}
