using Microsoft.VisualBasic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

#nullable enable
namespace HabboGallery.Desktop.Web.Json
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public bool TryGetData([NotNullWhen(true)]out T? data)
        {
            data = Data!;
            return string.IsNullOrEmpty(Error);
        }
    }
}
