using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

using HabboGallery.Desktop.Web.Json;

using Sulakore.Habbo;

#nullable enable
namespace HabboGallery.Desktop.Habbo
{
    /// <summary>
    /// Represents single photo item in-game.
    /// </summary>
    public class PhotoItem
    {
        public const int DATE_LENGTH = 14;
        public const string DATE_FORMAT = "dd/MM/yy HH:mm";

        private static readonly Regex _extradataPattern = new(@"^(?<Checksum>-?\d+)\s(?<DateTime>\d+\/\d+\/\d+\s\d+:\d+)\s(?<Description>.*)$", RegexOptions.Compiled);

        [JsonPropertyName("item_id")]
        public int Id { get; set; }

        [JsonPropertyName("game_checksum")]
        public int Checksum { get; set; }

        [JsonPropertyName("owner_name")]
        public string? OwnerName { get; set; }

        [JsonPropertyName("room_id")]
        public int? RoomId { get; set; }

        [JsonPropertyName("country_code")]
        [JsonConverter(typeof(HotelConverter))]
        public HHotel Hotel { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTime Date { get; set; }

        public PhotoItem()
        { }

        public static bool Validate(string extraData)
            => _extradataPattern.IsMatch(extraData);

        public static PhotoItem Create(int id, string data, HHotel hotel, string? ownerName, int? roomId)
        {
            Match match = _extradataPattern.Match(data);

            return new PhotoItem
            {
                Id = id,
                Checksum = int.Parse(match.Groups["Checksum"].Value),
                OwnerName = ownerName,
                RoomId = roomId,
                Date = DateTime.ParseExact(match.Groups["DateTime"].Value, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None),
                Description = match.Groups["Description"].Value,
                Hotel = hotel
            };
        }
    }
}