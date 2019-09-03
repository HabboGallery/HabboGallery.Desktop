using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using HabboGallery.Properties;
using System.Diagnostics;
using Eavesdrop;

namespace HabboGallery.UI
{
    public class UIUpdater
    {
        private int _zoomIndex;
        private HashSet<Control> _controlsToMove;
        private PrivateFontCollection _fontCollection;
        private readonly IReadOnlyList<string> _zoomTypes;

        private ButtonFlash _buttonFlashRunning;

        private const int WmNclbuttondown = 0xA1;
        private const int HtCaption = 0x2;
        private const int WmLbuttondown = 0x0201;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        public MainFrm Target { get; }

        public UIUpdater(MainFrm target)
        {
            Target = target;
            _zoomTypes = new List<string> { "2X", "1X" }.AsReadOnly();

            InitFont();
            InitDraggableControls();
            CheckForUpdates();
        }

        private void InitDraggableControls()
        {
            Application.AddMessageFilter(Target);

            _controlsToMove = new HashSet<Control> { Target, Target.DragPnl };
        }

        private async void CheckForUpdates()
        {
            var newVersion = await Target.ApiClient.GetLatestVersionAsync();

            if (newVersion > Constants.APP_VERSION)
            {
                var result = MessageBox.Show(Constants.UPDATE_FOUND_BODY, Constants.UPDATE_FOUND_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (result != DialogResult.Yes)
                    return;

                Process.Start(Constants.BASE_URL + Constants.UPDATE_URL);
                Eavesdropper.Terminate();
                Environment.Exit(0);
            }
            else
            {
                SetStatusMessage(Constants.UP_TO_DATE_MESSAGE);
            }
        }

        public bool DragControl(ref Message m)
        {
            if (m.Msg == WmLbuttondown && _controlsToMove.Contains(Control.FromHandle(m.HWnd)))
            {
                ReleaseCapture();
                SendMessage(Target.Handle, WmNclbuttondown, HtCaption, 0);
                return true;
            }
            return false;
        }

        private void InitFont()
        {
            _fontCollection = new PrivateFontCollection();

            byte[] fontdata = Resources.volter;
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
                Target.AutoLoginBx.Font =
                font;
        }

        public void Update()
        {
            Target.Invoke((MethodInvoker)delegate
            {
                var photo = Target.Photos.Count > 0 ? Target.CurrentPhoto : null;
                bool hasPhotos = photo != null;

                Target.PreviousPhotoBtn.Enabled = Target.CurrentIndex > 0;
                Target.PreviousPhotoBtn.BackgroundImage = Target.PreviousPhotoBtn.Enabled ? Resources.PreviousButton : Resources.DisabledPreviousButton;

                Target.NextPhotoBtn.Enabled = Target.CurrentIndex < Target.Photos.Count - 1 && Target.Photos.Count > 1;
                Target.NextPhotoBtn.BackgroundImage = Target.NextPhotoBtn.Enabled ? Resources.ForwardButton : Resources.DisabledForwardButton;

                Target.IndexDisplayLbl.Text = $"{Target.CurrentIndex + 1}/{Target.Photos.Count}";
                Target.DescriptionLbl.Text = hasPhotos ? photo.Description : string.Empty;

                Target.BuyPhotoBtn.Enabled = true;
                Target.BuyPhotoBtn.BackgroundImage = Target.BuyPhotoBtn.Enabled ? Resources.BuyButton : Resources.DisabledBuyButton;

                Target.PublishToWebBtn.Enabled = true;
                Target.PublishToWebBtn.BackgroundImage = Target.PublishToWebBtn.Enabled ? Resources.PublishButton : Resources.DisabledPublishButton;

                if (hasPhotos)
                {
                    Target.PhotoPreviewBx.Image = Target.ImageCache[photo.Id];
                    Target.PhotoPreviewBx.BackColor = Color.Black;
                }
                Target.BuyType = 0;
            });
        }

        public void UpdateRoomItemsQueueStatusMessage()
            => SetStatusMessage(Target.RoomItemsQueue.Count > 0 ? $"Photos in queue: {Target.RoomItemsQueue.Count}" : "No photos queued!");

        public void UpdateInventoryQueueStatusMessage(int queueCount)
            => SetStatusMessage(queueCount > 0 ? $"Photos in queue: {queueCount}" : "No photos queued!");

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
                _zoomIndex = (_zoomIndex - 1) < 0 ? _zoomTypes.Count - 1 : _zoomIndex - 1;
            else
                _zoomIndex = (_zoomIndex + 1) == _zoomTypes.Count ? 0 : _zoomIndex + 1;

            Target.Invoke((MethodInvoker)delegate
            {
                Target.ZoomLbl.Text = _zoomTypes[_zoomIndex];
            });
        }

        public int GetZoomValue()
            => _zoomIndex == 0 ? 2 : 1;

        public async void FlashButton(ButtonFlash button)
        {
            Panel uiButton = Target.PublishToWebBtn;
            Image enabledImage = uiButton.BackgroundImage;
            Image disabledImage = uiButton.BackgroundImage;
            bool enabledOnFlashStop = false;
            int flashCounter = 0;

            _buttonFlashRunning = button;

            switch (button)
            {
                case ButtonFlash.InventorySearch:
                {
                    uiButton = Target.SearchBtn;
                    enabledImage = Resources.SearchButton;
                    disabledImage = Resources.DisabledSearchButton;
                    enabledOnFlashStop = true;
                    break;
                }
                case ButtonFlash.Purchase:
                {
                    uiButton = Target.BuyPhotoBtn;
                    enabledImage = Resources.BuyButton;
                    disabledImage = Resources.DisabledBuyButton;
                    break;
                }
                case ButtonFlash.PublishToWeb:
                {
                    uiButton = Target.PublishToWebBtn;
                    enabledImage = Resources.PublishButton;
                    disabledImage = Resources.DisabledPublishButton;
                    break;
                }
            }

            if (button == ButtonFlash.None)
                return;

            while (_buttonFlashRunning == button)
            {
                uiButton.BackgroundImage = flashCounter % 2 == 0 ? enabledImage : disabledImage;
                flashCounter = (flashCounter % (int.MaxValue)) + 1;
                await Task.Delay(500);
            }

            uiButton.BackgroundImage = enabledOnFlashStop ? enabledImage : disabledImage;
        }

        public void SetLoginMessage(string message, bool enableButton)
        {
            Target.Invoke(new MethodInvoker(() =>
            {
                Target.LoginErrorLbl.Text = message;
                Target.LoginSubmitBtn.Enabled = enableButton;
            }));
        }

        public void HideLogin() 
            => Target.LoginPnl.Visible = false;

        public void ToggleSearchButton(bool enable)
        {
            Target.Invoke((MethodInvoker)delegate
            {
                Target.SearchBtn.Enabled = enable;
                Target.SearchBtn.BackgroundImage = enable ? Resources.SearchButton : Resources.DisabledSearchButton;
            });
        }
    }
}
