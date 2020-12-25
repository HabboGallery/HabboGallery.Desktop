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
        public static HGResources Resources => Program.Master.Resources;

        private readonly HashSet<Control> _controlsToMove;
        private readonly PrivateFontCollection _fontCollection;

        private readonly string[] _zoomTypes = new string[2] { "2X", "1X" };

        private int _zoomIndex;

        public MainFrm Target { get; }

        public UIUpdater(MainFrm target)
        {
            Target = target;

            // Initialize draggable controls
            _controlsToMove = new HashSet<Control> {
                Target,
                Target.DragPnl
            };
            
            // Initialize Volter font
            _fontCollection = new PrivateFontCollection();
            InitVolterFont();
        }

        private void InitVolterFont()
        {
            byte[] fontdata = HGResources.GetResourceBytes("Volter.ttf");

            IntPtr fontDataPtr = Marshal.AllocCoTaskMem(fontdata.Length);
            Marshal.Copy(fontdata, 0, fontDataPtr, fontdata.Length);

            uint cFonts = 0;
            NativeMethods.AddFontMemResourceEx(fontDataPtr, (uint)fontdata.Length, IntPtr.Zero, ref cFonts);
            _fontCollection.AddMemoryFont(fontDataPtr, fontdata.Length);

            Marshal.FreeCoTaskMem(fontDataPtr);

            Target.DescriptionLbl.Font = 
                Target.EmailLbl.Font =
                Target.PasswordLbl.Font =
                Target.LoginEmailTxt.Font =
                Target.LoginPasswordTxt.Font =
                Target.LoginTitleLbl.Font =
                Target.IndexDisplayLbl.Font = 
                Target.ZoomLbl.Font = 
                Target.StatusLbl.Font = 
                Target.RememberMeBx.Font = new Font(_fontCollection.Families[0], 7);
        }

        public bool DragControl(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int WM_LBUTTONDOWN = 0x0201;
            const int HTCAPTION = 0x2;

            if (m.Msg == WM_LBUTTONDOWN && _controlsToMove.Contains(Control.FromHandle(m.HWnd)))
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Target.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                return true;
            }
            return false;
        }

        public void OnPhotoQueueUpdate()
        {
            Target.Invoke((MethodInvoker)delegate
            {
                bool photoSelected = Target.Photos.Count > 0;

                Target.IndexDisplayLbl.Text = Target.CurrentIndex + 1 + "/" + Target.Photos.Count;

                Target.PreviousBtn.Enabled = Target.CurrentIndex > 0;
                Target.NextBtn.Enabled = Target.CurrentIndex < Target.Photos.Count - 1 && Target.Photos.Count > 1;
                
                Target.BuyBtn.Enabled = photoSelected;
                Target.PublishBtn.Enabled = photoSelected;
                
                Resources.RenderButtonState(Target.PreviousBtn, Target.PreviousBtn.Enabled);
                Resources.RenderButtonState(Target.NextBtn, Target.NextBtn.Enabled);

                Resources.RenderButtonState(Target.BuyBtn, Target.BuyBtn.Enabled);
                Resources.RenderButtonState(Target.PublishBtn, Target.PublishBtn.Enabled);

                if (photoSelected)
                {
                    Target.PhotoPreviewBx.Image = Target.ImageCache[Target.CurrentPhoto.Id];
                    Target.PhotoPreviewBx.BackColor = Color.Black;

                    Target.DescriptionLbl.Text = Target.CurrentPhoto.Description;
                }
                else Target.DescriptionLbl.Text = string.Empty;

                Target.BuyType = 0; //TODO: ?
            });
        }

        public void UpdateQueueStatus(int queueCount) 
            => SetStatusMessage(queueCount > 0 ? $"Photos in queue: {queueCount}" : "No photos queued!");

        public void SetStatusMessage(string message)
        {
            Target.Invoke((MethodInvoker)delegate
            {
                Target.StatusLbl.Text = message;
            });
        }

        //TODO: Refactor entire carousel logic, too tired to do this at 3am
        public void RotateZoomCarousel(CarouselDirection direction)
        {
            if (direction == CarouselDirection.Up)
                _zoomIndex = (_zoomIndex - 1) < 0 ? _zoomTypes.Length - 1 : _zoomIndex - 1;
            else  _zoomIndex = (_zoomIndex + 1) == _zoomTypes.Length ? 0 : _zoomIndex + 1;

            Target.ZoomLbl.Invoke((MethodInvoker)delegate
            {
                Target.ZoomLbl.Text = _zoomTypes[_zoomIndex];
            });
        }

        public int GetZoomValue() => _zoomIndex == 0 ? 2 : 1;

        public async Task StartFlashingButtonAsync(ButtonFlash button, CancellationToken cancellationToken = default)
        {
            const int PHOTO_FLASH_DELAY = 500;

            Control buttonToFlash = button switch
            {
                ButtonFlash.InventorySearch => Target.SearchBtn,
                ButtonFlash.PublishToWeb => Target.PublishBtn,
                ButtonFlash.Purchase => Target.BuyBtn,
                ButtonFlash.NextPhoto => Target.NextBtn,
                ButtonFlash.PreviousPhoto => Target.PreviousBtn,

                _ => throw new ArgumentException(null, nameof(button))
            };

            bool flashToggle = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                Resources.RenderButtonState(buttonToFlash, flashToggle = !flashToggle);
                await Task.Delay(PHOTO_FLASH_DELAY, cancellationToken).ConfigureAwait(false);
            }
            
            Resources.RenderButtonState(buttonToFlash, button == ButtonFlash.InventorySearch); //TODO: sets others to disabled state, is this fine?
        }

        public void SetLoginMessage(string message, bool enableButton)
        {
            Target.Invoke((MethodInvoker)delegate 
            {
                Target.LoginErrorLbl.Text = message;
                Target.LoginSubmitBtn.Enabled = enableButton;
            });
        }

        public void HideLogin() => Target.LoginPnl.Visible = false;
    }
}
