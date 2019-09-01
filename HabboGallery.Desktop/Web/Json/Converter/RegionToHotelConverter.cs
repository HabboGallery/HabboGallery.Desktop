using System;

using Sulakore.Habbo;
using Sulakore.Network;

using Newtonsoft.Json;

namespace HabboGallery.Web.Json
{
    public sealed class RegionToHotelConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType != typeof(string)) return null;
            return HotelEndPoint.GetHotel((string)reader.Value);
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        
        public override bool CanConvert(Type objectType)
            => objectType == typeof(HHotel);
    }
}
