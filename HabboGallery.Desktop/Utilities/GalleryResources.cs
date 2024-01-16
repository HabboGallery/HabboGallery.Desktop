namespace HabboGallery.Desktop.Utilities;

public partial class GalleryResources
{
    [EmbedResourceCSharp.FolderEmbed("Resources/")]
    public static partial ReadOnlySpan<byte> GetResourceBytes(ReadOnlySpan<char> path);

    public unsafe static Image GetImage(ReadOnlySpan<char> path)
    {
        var data = GetResourceBytes(path);
        fixed (byte* dataPtr = data)
        {
            using UnmanagedMemoryStream stream = new UnmanagedMemoryStream(dataPtr, data.Length);
            return Image.FromStream(stream);
        }
    }
}
