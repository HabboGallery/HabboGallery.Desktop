using System;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HabboGallery.Web.Json;
using HabboGallery.Properties;

using Sulakore.Habbo;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HabboGallery.Habbo
{
    public class OldPhoto
    {
        public const string DATE_FORMAT = "dd/MM/yy HH:mm";
        private const string ExtradataPattern = @"^(?<Checksum>-?\d+)\s(?<DateTime>\d+\/\d+\/\d+\s\d+:\d+)\s(?<Description>.*)$";

        #region Json Properties
        [JsonProperty("item_id")]
        public int Id { get; set; }

        [JsonProperty("game_checksum")]
        public int Checksum { get; set; }

        [JsonProperty("date")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }

        public string Description { get; set; }
        public string Url { get; set; }
        [JsonProperty("str_fill")]
        public string StrFill { get; set; }
        public int RoomId { get; set; }

        [JsonProperty("country_code")]
        [JsonConverter(typeof(RegionToHotelConverter))]
        public HHotel Hotel { get; set; }
        #endregion

        private static readonly Dictionary<char, Image> CharacterImages = new Dictionary<char, Image>()
        {
            {'0', Resources.Zero },
            {'1', Resources.One },
            {'2', Resources.Two },
            {'3', Resources.Three },
            {'4', Resources.Four },
            {'5', Resources.Five },
            {'6', Resources.Six },
            {'7', Resources.Seven },
            {'8', Resources.Eight },
            {'9', Resources.Nine },
            {'/', Resources.ForwardSlash },
            {':', Resources.Colon },
            {' ', Resources.EmptySpaceCharacter },
        };

        public long UnixTime  => new DateTimeOffset(Date).ToUnixTimeSeconds();

        public OldPhoto() { }
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

            Image newImage = new Bitmap(dateLength + 2, 9);

            int offset = 0;
            using (Graphics g = Graphics.FromImage(newImage))
            {
                foreach (char c in date)
                {
                    Image theChar = CharacterImages[c];

                    g.DrawImage(theChar, new Point(offset, 0));
                    offset += theChar.Width - (c == ' ' ? 0 : 1);
                }
            }

            int yOffset = photo.Height - 12;
            int xOffset = photo.Width - 5 - newImage.Width;

            using (Graphics g = Graphics.FromImage(photo))
            {
                g.DrawImage(newImage, new Point(xOffset, yOffset));
            }

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
        {
            return JsonConvert.SerializeObject(this);
        }

        public static OldPhoto FromJson(string json)
        {
            return JsonConvert.DeserializeObject<OldPhoto>(json);
        }
    }
}
