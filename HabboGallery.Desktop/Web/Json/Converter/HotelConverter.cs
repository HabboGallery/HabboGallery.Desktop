using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Sulakore.Habbo;
using Sulakore.Network;

namespace HabboGallery.Desktop.Web.Json;

public sealed class HotelConverter : JsonConverter<HHotel>
{
    public override HHotel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.String ? 
            HotelEndPoint.GetHotel(reader.GetString()) : HHotel.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, HHotel value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(HotelEndPoint.GetRegion(value));
    }
}
