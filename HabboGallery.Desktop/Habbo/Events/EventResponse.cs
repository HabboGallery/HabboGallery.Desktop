using System.Text.Json;
using System.Text.Json.Serialization;

using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo.Events
{
    public class EventResponse
    {
        private const string EVENT_PREFIX = "friendbar/user/";

        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonPropertyName("data")]
        public string Data { get; private set; }

        public EventResponse(string name, string data)
        {
            Name = name;
            Data = data;
        }

        public static EventResponse Parse(HPacket packet)
        {
            return JsonSerializer.Deserialize<EventResponse>(packet.ReadUTF8());
        }
        public string ToEventString()
        {
            return EVENT_PREFIX + JsonSerializer.Serialize(this);
        }
    }
}
