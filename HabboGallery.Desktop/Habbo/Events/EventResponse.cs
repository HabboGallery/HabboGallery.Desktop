using Newtonsoft.Json;
using Sulakore.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabboGallery.Habbo.Events
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EventResponse
    {
        private const string EVENT_PREFIX = "friendbar/user/";

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("data")]
        public string Data { get; private set; }

        public EventResponse(string name, string data)
        {
            Name = name;
            Data = data;
        }

        public static EventResponse Parse(HPacket packet)
        {
            string json = packet.ReadUTF8();
            return JsonConvert.DeserializeObject<EventResponse>(json);
        }

        public string ToEventString()
        {
            return EVENT_PREFIX + JsonConvert.SerializeObject(this);
        }


    }
}
