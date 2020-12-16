using System.Text.Json;
using System.Text.Json.Serialization;

namespace HabboGallery.Desktop.Habbo.Events
{
    public class EventResponse
    {
        private const string EVENT_PREFIX = "friendbar/user/";

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        public EventResponse()
        { }
        public EventResponse(string name, string data)
        {
            Name = name;
            Data = data;
        }

        public string ToEventString() => EVENT_PREFIX + JsonSerializer.Serialize(this);
    }
}
