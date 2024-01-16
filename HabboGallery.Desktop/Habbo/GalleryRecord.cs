using System.Globalization;
using System.Text.Json.Serialization;

using HabboGallery.Desktop.Utilities;

namespace HabboGallery.Desktop.Habbo;

/// <summary>
/// Represents a photo that has been published on a web server.
/// </summary>
public record GalleryRecord : PhotoItem
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("str_fill")]
    public string StrFill { get; set; }

    private static readonly Dictionary<char, Image> CharacterImages = new()
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

    public Image CreateDateImage(Image photo)
    {
        Span<char> date = stackalloc char[DATE_LENGTH];
        Date.TryFormat(date, out int charsWritten, DATE_FORMAT, CultureInfo.InvariantCulture);

        int dateWidth = 0;
        foreach (char c in date)
        {
            dateWidth += CharacterImages[c].Width - 1;
        }

        using Graphics graphics = Graphics.FromImage(photo);

        int dateY = photo.Height - 12;
        int dateX = photo.Width - 5 - (dateWidth + 2);
        foreach (char c in date)
        {
            Image theChar = CharacterImages[c];

            graphics.DrawImage(theChar, dateX, dateY);
            dateX += theChar.Width - 1;
        }

        return photo;
    }
}
