using System.Globalization;
using System.Text.Json.Serialization;

using HabboGallery.Desktop.Utilities;

namespace HabboGallery.Desktop.Habbo;

/// <summary>
/// Represents a photo that has been published on a web server.
/// </summary>
public sealed record GalleryRecord : PhotoItem
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("str_fill")]
    public string StrFill { get; set; }

    private static readonly Dictionary<char, Image> CharacterImages = new()
    {
        {'0', GalleryResources.GetImage("0.png")},
        {'1', GalleryResources.GetImage("1.png")},
        {'2', GalleryResources.GetImage("2.png")},
        {'3', GalleryResources.GetImage("3.png")},
        {'4', GalleryResources.GetImage("4.png")},
        {'5', GalleryResources.GetImage("5.png")},
        {'6', GalleryResources.GetImage("6.png")},
        {'7', GalleryResources.GetImage("7.png")},
        {'8', GalleryResources.GetImage("8.png")},
        {'9', GalleryResources.GetImage("9.png")},
        {'/', GalleryResources.GetImage("Slash.png")},
        {':', GalleryResources.GetImage("Colon.png")},
        {' ', GalleryResources.GetImage("Space.png")},
    };

    // TODO: ImageSharp
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
