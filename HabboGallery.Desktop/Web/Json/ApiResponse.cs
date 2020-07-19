using System.Text.Json.Serialization;

namespace HabboGallery.Desktop.Web.Json
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        public bool Success { get; set; }
    }
}
