using System;
using System.Drawing;
using System.Text.Json;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

using HabboGallery.Desktop.Web.Json;
using HabboGallery.Desktop.Utilities;

using Sulakore.Habbo;

namespace HabboGallery.Desktop.Habbo
{
    public class OldPhoto
    {
        public const string DATE_FORMAT = "dd/MM/yy HH:mm";
        private const string ExtradataPattern = @"^(?<Checksum>-?\d+)\s(?<DateTime>\d+\/\d+\/\d+\s\d+:\d+)\s(?<Description>.*)$";
        private static readonly Regex _compiledExtradataRegex = new Regex(ExtradataPattern, RegexOptions.Compiled);

        #region Json Properties
        [JsonPropertyName("item_id")]
        public int Id { get; set; }

        [JsonPropertyName("checksum")] //TODO: game_checksum => checksum
        public int Checksum { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("str_fill")]
        public string StrFill { get; set; }

        [JsonPropertyName("room_id")] //TODO:
        public int RoomId { get; set; }

        [JsonPropertyName("country_code")]
        [JsonConverter(typeof(HotelConverter))]
        public HHotel Hotel { get; set; }
        #endregion

        private static readonly Dictionary<char, Image> CharacterImages = new Dictionary<char, Image>()
        {
            {'0', Image.FromStream(HGResources.GetResourceStream("0.png"))},
            {'1', Image.FromStream(HGResources.GetResourceStream("1.png"))},
            {'2', Image.FromStream(HGResources.GetResourceStream("2.png"))},
            {'3', Image.FromStream(HGResources.GetResourceStream("3.png"))},
            {'4', Image.FromStream(HGResources.GetResourceStream("4.png"))},
            {'5', Image.FromStream(HGResources.GetResourceStream("5.png"))},
            {'6', Image.FromStream(HGResources.GetResourceStream("6.png"))},
            {'7', Image.FromStream(HGResources.GetResourceStream("7.png"))},
            {'8', Image.FromStream(HGResources.GetResourceStream("8.png"))},
            {'9', Image.FromStream(HGResources.GetResourceStream("9.png"))},
            {'/', Image.FromStream(HGResources.GetResourceStream("Slash.png"))},
            {':', Image.FromStream(HGResources.GetResourceStream("Colon.png"))},
            {' ', Image.FromStream(HGResources.GetResourceStream("Space.png"))},
        };

        public long PhotoUnixTime => new DateTimeOffset(Date).ToUnixTimeSeconds();

        public OldPhoto() 
        { }
        public OldPhoto(int id, int checksum, string datetime, string description, HHotel hotel)
        {
            Id = id;
            Hotel = hotel;
            Checksum = checksum;
            Description = description;

            Date = DateTime.ParseExact(datetime, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public Image CreateDateImage(Image photo)
        {
            string date = Date.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);

            int dateLength = 0;
            foreach (char c in date)
            {
                dateLength += CharacterImages[c].Width - (c == ' ' ? 0 : 1);
            }

            using Image newImage = new Bitmap(dateLength + 2, 9);
            using Graphics g = Graphics.FromImage(newImage);

            int offset = 0;
            foreach (char c in date)
            {
                Image theChar = CharacterImages[c];

                g.DrawImage(theChar, offset, 0);
                offset += theChar.Width - (c == ' ' ? 0 : 1);
            }

            int yOffset = photo.Height - 12;
            int xOffset = photo.Width - 5 - newImage.Width;

            using Graphics newImageGraphics = Graphics.FromImage(photo); //TODO: ^^ Draw directly to photo
            newImageGraphics.DrawImage(newImage, xOffset, yOffset);

            return photo;
        }

        public static bool Validate(string extraData)
            => Regex.IsMatch(extraData, ExtradataPattern);

        public static OldPhoto Parse(int id, string data, HHotel hotel)
        {
            Match m = Regex.Match(data, ExtradataPattern);
            return new OldPhoto(id, int.Parse(m.Groups["Checksum"].Value),
                m.Groups["DateTime"].Value, m.Groups["Description"].Value, hotel);
        }

        public override string ToString()
            => $"Id: {Id}\r\nChecksum: {Checksum}\r\nDate: {Date}\r\nDescription: {Description}";

        public string ToJson() 
            => JsonSerializer.Serialize(this);

        public static OldPhoto FromJson(string json)
            => JsonSerializer.Deserialize<OldPhoto>(json);
    }
}
