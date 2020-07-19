using System;
using System.Drawing;
using System.Threading;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using HabboGallery.Desktop.Utilities;

namespace HabboGallery.Desktop.UI
{
    public class UIUpdater
    {
        public HGResources Resources => Program.Master.Resources;

        private readonly HashSet<Control> _controlsToMove;
        private readonly PrivateFontCollection _fontCollection;

        private readonly string[] _zoomTypes = new string[2] { "2X", "1X" };

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
        
        private int _zoomIndex;

        public MainFrm Target { get; }

        public UIUpdater(MainFrm target)
        {
            Target = target;

            // Initialize draggable controls
            Application.AddMessageFilter(Target);
            _controlsToMove = new HashSet<Control> { Target, Target.DragPnl };
            
            // Initialize Volter font
            _fontCollection = new PrivateFontCollection();
            InitFonts();
        }

        private void InitFonts()
        {
            byte[] fontdata = Resources.GetResourceBytes("Volter.ttf");

            IntPtr data = Marshal.AllocCoTaskMem(fontdata.Length);
            Marshal.Copy(fontdata, 0, data, fontdata.Length);

            uint cFonts = 0;
            AddFontMemResourceEx(data, (uint)fontdata.Length, IntPtr.Zero, ref cFonts);
            _fontCollection.AddMemoryFont(data, fontdata.Length);

            Marshal.FreeCoTaskMem(data);

            var font = new Font(_fontCollection.Families[0], 7);

            Target.DescriptionLbl.Font = 
                Target.IndexDisplayLbl.Font = 
                Target.ZoomLbl.Font = 
                Target.StatusLbl.Font = 
                Target.LoginEmailTxt.Font =
                Target.RememberMeBx.Font = font;
        }

        public bool DragControl(ref Message m)
        {
            const int WmNclbuttondown = 0xA1;
            const int WmLbuttondown = 0x0201;
            const int HtCaption = 0x2;

            if (m.Msg == WmLbuttondown && _controlsToMove.Contains(Control.FromHandle(m.HWnd)))
            {
                ReleaseCapture();
                SendMessage(Target.Handle, WmNclbuttondown, HtCaption, 0);
                return true;
            }
            return false;
        }

        public void OnPhotoQueueUpdate()
        {
            Target.PreviousBtn.Enabled = Target.CurrentIndex > 0;
            Resources.RenderButtonState(Target.PreviousBtn, Target.PreviousBtn.Enabled);

            Target.NextBtn.Enabled = Target.CurrentIndex < Target.Photos.Count - 1 && Target.Photos.Count > 1;
            Resources.RenderButtonState(Target.NextBtn, Target.NextBtn.Enabled);

            Target.BuyBtn.Enabled = true;
            Resources.RenderButtonState(Target.BuyBtn, true);
            
            Target.PublishBtn.Enabled = true;
            Resources.RenderButtonState(Target.PublishBtn, true);
            
            Target.Invoke((MethodInvoker)delegate
            {
                var photo = Target.Photos.Count > 0 ? Target.CurrentPhoto : null;
                bool hasPhotos = photo != null;

                Target.IndexDisplayLbl.Text = Target.CurrentIndex + 1 + "/" + Target.Photos.Count;
                Target.DescriptionLbl.Text = hasPhotos ? photo.Description : string.Empty;

                if (hasPhotos)
                {
                    Target.PhotoPreviewBx.Image = Target.ImageCache[photo.Id];
                    Target.PhotoPreviewBx.BackColor = Color.Black;
                }
                Target.BuyType = 0;
            });
        }

        public void UpdateQueueStatus(int queueCount) => SetStatusMessage(queueCount > 0 ? $"Photos in queue: {queueCount}" : "No photos queued!");

        public void SetStatusMessage(string message)
        {
            Target.Invoke((MethodInvoker)delegate
            {
                Target.StatusLbl.Text = message;
            });
        }

        public void RotateZoomCarousel(CarouselDirection direction)
        {
            if (direction == CarouselDirection.Up)
                _zoomIndex = (_zoomIndex - 1) < 0 ? 
                    _zoomTypes.Length - 1 : _zoomIndex - 1;
            else
                _zoomIndex = (_zoomIndex + 1) == _zoomTypes.Length ?
                    0 : _zoomIndex + 1;

            Target.Invoke((MethodInvoker)delegate
            {
                Target.ZoomLbl.Text = _zoomTypes[_zoomIndex];
            });
        }

        public int GetZoomValue() => _zoomIndex == 0 ? 2 : 1;

        public async Task StartFlashingButtonAsync(ButtonFlash button, CancellationToken cancellationToken = default)
        {
            if (button == ButtonFlash.None) return;

            Control buttonToFlash = button switch
            {
                ButtonFlash.InventorySearch => Target.SearchBtn,
                ButtonFlash.PublishToWeb => Target.PublishBtn,
                ButtonFlash.Purchase => Target.BuyBtn,
                ButtonFlash.NextPhoto => Target.NextBtn,
                ButtonFlash.PreviousPhoto => Target.PreviousBtn,

                _ => throw new ArgumentException(nameof(button))
            };

            bool flashToggle = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                Resources.RenderButtonState(buttonToFlash, flashToggle = !flashToggle);
                await Task.Delay(500).ConfigureAwait(false);
            }
            
            Resources.RenderButtonState(buttonToFlash, button == ButtonFlash.InventorySearch); //TODO: sets others false, is this fine?
        }

        public void SetLoginMessage(string message, bool enableButton)
        {
            Target.Invoke(new MethodInvoker(() =>
            {
                Target.LoginErrorLbl.Text = message;
                Target.LoginSubmitBtn.Enabled = enableButton;
            }));
        }

        public void HideLogin() => Target.LoginPnl.Visible = false;
    }
}
