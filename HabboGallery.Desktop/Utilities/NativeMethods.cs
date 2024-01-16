using System.Runtime.InteropServices;

namespace HabboGallery.Desktop.Utilities;

internal unsafe static class NativeMethods
{
    [DllImport("user32.dll")]
    internal static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    internal static extern int SendMessage(void* hWnd, int Msg, int wParam, int lParam);
    
    [DllImport("gdi32.dll")]
    internal static extern nint AddFontMemResourceEx(void* pbFont, uint cbFont, void* pdv, uint* pcFonts);
}
