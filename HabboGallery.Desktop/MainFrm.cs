using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using HabboGallery.Desktop.UI;
using HabboGallery.Desktop.Web;
using HabboGallery.Desktop.Habbo;
using HabboGallery.Desktop.Controls;
using HabboGallery.Desktop.Web.Json;
using HabboGallery.Desktop.Utilities;
using HabboGallery.Desktop.Habbo.Network;

using Sulakore.Habbo;
using Sulakore.Network;
using Sulakore.Habbo.Messages;

using Eavesdrop;

#nullable enable
namespace HabboGallery.Desktop
{
    public partial class MainFrm : NotifiableForm, IMessageFilter
    {
        public readonly Version Version = new Version(1, 0, 0);

        private const int GET_ITEMDATA_DELAY = 780;
        private const int PUBLISH_QUEUE_POLL_DELAY = 1000;

        public ApiClient Api => Master.Api;
        
        public Incoming In => Master.In;
        public Outgoing Out => Master.Out;

        public HGResources Resources => Master.Resources;
        public HGConfiguration Configuration => Master.Configuration;

        public HConnection Connection => Master.Connection;

        private readonly UIUpdater _ui;
        
        //TODO: Lazy loading the images in carousel
        public Dictionary<int, Image> ImageCache { get; }
        public Dictionary<int, GalleryRecord> Photos { get; }

        public int CurrentIndex { get; set; }
        public GalleryRecord? CurrentPhoto => Photos?.Values.ToArray()[CurrentIndex]; //TODO:

        private Queue<int> _roomPhotoQueue;
        private Queue<PhotoItem> _photoPublishingQueue;
        private Dictionary<int, HWallItem> _roomPhotoItems;
        
        public string? SessionUsername { get; set; }
        public int CurrentRoomId { get; private set; }

        private bool _isProcessingItems;

        public bool HasBeenClosed { get; private set; }

        public HHotel Hotel => HotelServer.Hotel;

        public MainFrm()
        {
            Application.AddMessageFilter(this);

            ImageCache = new Dictionary<int, Image>();
            Photos = new Dictionary<int, GalleryRecord>();
            
            _roomPhotoQueue = new Queue<int>();
            _photoPublishingQueue = new Queue<PhotoItem>();

            _roomPhotoItems = new Dictionary<int, HWallItem>();

            InitializeComponent();

            _ui = new UIUpdater(this);
            _ = CheckForUpdatesAsync();
        }

        public bool PreFilterMessage(ref Message msg)
            => _ui.DragControl(ref msg);

        private async Task CheckForUpdatesAsync()
        {
            Version? newVersion = await Api.GetLatestVersionAsync().ConfigureAwait(false);

            if (newVersion > Version)
            {
                var result = MessageBox.Show(Constants.UPDATE_FOUND_BODY, Constants.UPDATE_FOUND_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    Eavesdropper.Terminate();
                    Process.Start(new ProcessStartInfo(Constants.BASE_URL + "/download") { UseShellExecute = true });

                    Environment.Exit(0);
                }
            }
            else _ui.SetStatusMessage(Constants.UP_TO_DATE_MESSAGE);
        }
        
        private void Initialize()
        {
            Connection.DataOutgoing += HandleOutgoing;
            Connection.DataIncoming += HandleIncoming;

            Connection.Connected += ConnectionOpened;
            Connection.Disconnected += ConnectionClosed;

            if (Eavesdropper.Certifier.CreateTrustedRootCertificate())
            {
                Eavesdropper.ResponseInterceptedAsync += InterceptClientPageAsync;

                Eavesdropper.Initiate(Constants.PROXY_PORT);
                _ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT_PAGE);
            }
        }
        
        private void OnFurniList(DataInterceptedEventArgs e)
        {
            e.Continue();

            IEnumerable<CHItem> items = CHItem.Parse(e.Packet)
                .Where(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id));

            int itemCount = items.Count();

            string message = (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
                + (itemCount == 1 ? Constants.SCANNING_SINGLE : Constants.SCANNING_MULTI)
                + Constants.SCANNING_INVENTORY_DONE;
            
            //TODO: message
            //TODO: Show user the queueu in photo publishing pipeline

            _isProcessingItems = true;

            //Send all photo items in inventory to photo data processing pipeline.
            foreach (CHItem item in items)
            {
                var photoItem = PhotoItem.Create(item.Id, item.ExtraData, Hotel, SessionUsername,
                    roomId: null);

                _photoPublishingQueue.Enqueue(photoItem);
            }
        }
        private async void OnItems(DataInterceptedEventArgs e)
        {
            e.Continue();

            IEnumerable<HWallItem> items = HWallItem.Parse(e.Packet)
                .Where(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id) &&
                !_roomPhotoQueue.Contains(i.Id));

            IEnumerable<int>? unknownIds = await Api.BatchCheckExistingIdsAsync(items.Select(i => i.Id), Hotel).ConfigureAwait(false);

            int itemCount = items.Count();
            int unknownCount = unknownIds.Count();

            string alertMessage = (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
                + (itemCount == 0 || itemCount > 1 ? Constants.SCANNING_MULTI : Constants.SCANNING_SINGLE)
                + Constants.SCANNING_WALLITEMS_DONE;

            if (unknownCount > 0)
                alertMessage += " " + unknownCount + Constants.SCANNING_WALLITEMS_UNDISC;

            //TODO: alertMessage

            _isProcessingItems = true;

            foreach (HWallItem item in items)
            {
                if (unknownIds.Contains(item.Id))
                {
                    _roomPhotoQueue.Enqueue(item.Id);
                    _roomPhotoItems.TryAdd(item.Id, item);
                }
                else
                {
                    //TODO: Another pipeline.
                    await GetKnownPhotoByIdAsync(item.Id);
                }
            }
        }

        private static ReadOnlySpan<byte> ExtraDataPrefix => new byte[] { 192, 128, 192, 128 };
        private void OnItemDataUpdate(DataInterceptedEventArgs e)
        {
            e.Continue();

            int id = int.Parse(e.Packet.ReadUTF8());
            if (!_roomPhotoItems.ContainsKey(id)) return;

            //TODO: Investigate more, hhbr-r-72288657
            ReadOnlySpan<byte> extraDataRaw = e.Packet.ReadBytes(e.Packet.ReadUInt16());
            if (extraDataRaw.StartsWith(ExtraDataPrefix))
                extraDataRaw = extraDataRaw[4..];

            string extraData = Encoding.UTF8.GetString(extraDataRaw);
            try
            {
                if (PhotoItem.Validate(extraData) && !Photos.ContainsKey(id))
                {
                    _roomPhotoItems.TryGetValue(id, out HWallItem? roomItem);

                    var photoItem = PhotoItem.Create(id, extraData, Hotel, roomItem?.OwnerName, CurrentRoomId);
                    _photoPublishingQueue.Enqueue(photoItem);
                }
            }
            catch (FormatException) { }

            if (_roomPhotoQueue.Peek() == id)
            {
                _roomPhotoQueue.Dequeue();
                _ui.UpdateQueueStatus(_roomPhotoQueue.Count);
            }
        }

        private async Task HandlePhotoQueueAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(GET_ITEMDATA_DELAY, cancellationToken).ConfigureAwait(false);

                if (_roomPhotoQueue.Count > 0)
                    await ProcessPhotoQueueAsync().ConfigureAwait(false);
            }
        }
        public async Task ProcessPhotoItemsAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_photoPublishingQueue.TryDequeue(out PhotoItem? photoItem))
                {
                    await Task.Delay(PUBLISH_QUEUE_POLL_DELAY, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                ApiResponse<GalleryRecord>? storeResponse = await Api.StorePhotoAsync(photoItem).ConfigureAwait(false);
                if (storeResponse == null) return;

                if (!ImageCache.ContainsKey(photoItem.Id) &&
                    !Photos.ContainsKey(photoItem.Id) && 
                    storeResponse.TryGetData(out GalleryRecord? record))
                {
                    Photos.Add(photoItem.Id, record);

                    Image photoImage = await Api.TryGetImageAsync(record.Url, cancellationToken).ConfigureAwait(false);
                    ImageCache.Add(photoItem.Id, record.CreateDateImage(photoImage));

                    _ui.OnPhotoQueueUpdate();
                }
            }
        }
        private Task ProcessPhotoQueueAsync()
        {
            if (_roomPhotoQueue.Count <= 1)
                _isProcessingItems = false;

            return Connection.SendToServerAsync(Out.GetItemData, _roomPhotoQueue.Peek());
        }

        private async Task GetKnownPhotoByIdAsync(int photoId)
        {
            //TODO: check imageCache
            ApiResponse<GalleryRecord>? photoResponse = await Api.GetPhotoByIdAsync(photoId, Hotel).ConfigureAwait(false); //TODO: Create batch processing api
            if (photoResponse == null) return;

            if (!ImageCache.ContainsKey(photoId) && !Photos.ContainsKey(photoId) && photoResponse.TryGetData(out var record))
            {
                Photos.Add(photoId, record);

                Image photoImage = await Api.TryGetImageAsync(record.Url).ConfigureAwait(false);
                ImageCache.Add(photoId, record.CreateDateImage(photoImage));

                _ui.OnPhotoQueueUpdate();
            }
        }

        private void PreviousPhotoBtn_Click(object sender, EventArgs e)
        {
            CurrentIndex--;
            _ui.OnPhotoQueueUpdate();
        }
        private void NextPhotoBtn_Click(object sender, EventArgs e)
        {
            CurrentIndex++;
            _ui.OnPhotoQueueUpdate();
        }

        private void HotelExtensionUpBtn_Click(object sender, EventArgs e)
        {
            _ui.RotateZoomCarousel(CarouselDirection.Up);
        }
        private void HotelExtensionDownBtn_Click(object sender, EventArgs e)
        {
            _ui.RotateZoomCarousel(CarouselDirection.Down);
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Master.SaveConfig();

            TerminateProxy();
            Close();
        }

        private async void LoginSubmitBtn_Click(object sender, EventArgs e)
        {
            _ui.SetLoginMessage(string.Empty, false);
            try
            {
                bool isAuthenticated = await Api.LoginAsync(LoginEmailTxt.Text, LoginPasswordTxt.Text);
                if (isAuthenticated)
                {
                    if (RememberMeBx.Checked)
                    {
                        Configuration.Email = LoginEmailTxt.Text;
                        Configuration.Password = LoginPasswordTxt.Text;
                        Master.SaveConfig();
                    }

                    _ui.HideLogin();
                    Initialize();
                }
                else _ui.SetLoginMessage(Constants.LOGIN_FAILED, true);
            }
            catch (Exception)
            {
                _ui.SetLoginMessage(Constants.LOGIN_ERROR, true);
            }
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            TerminateProxy();
            LoadConfiguration();
        }

        private void RememberMeBx_CheckedChanged(object sender, EventArgs e)
        {
            Configuration.RememberMe = RememberMeBx.Checked;
        }

        private void LoadConfiguration()
        {
            if (Configuration.RememberMe)
            {
                LoginEmailTxt.Text = Configuration.Email;
                LoginPasswordTxt.Text = Configuration.Password;
                RememberMeBx.Checked = true;
            }
        }

        //TODO: Headers
        public void HandleOutgoing(object? sender, DataInterceptedEventArgs e)
        {
            if (HasBeenClosed)
            {
                e.Continue();

                //TODO: Allow graceful exit
                return;
            }

            if (e.Packet.Id == 4001)
            {
                //TODO: H2020

                e.IsBlocked = true;
            }
            else if (e.Packet.Id == Out.InfoRetrieve) //TODO: Clean up init
            {
                //TODO: CancellationTokenSauces fired here?
                _ = HandlePhotoQueueAsync();
                _ = ProcessPhotoItemsAsync();

                if (Configuration.IsFirstConnect)
                {
                    Configuration.IsFirstConnect = false;

                    //TODO: First connect alert
                    //Constants.INTRO_DIALOG_TITLE
                    //Constants.INTRO_DIALOG_BODY
                }
                else //TODO: Indicate successful connection in client somehow? Constants.APP_CONNECT_SUCCESS

                if (Hotel == HHotel.Com || Hotel == HHotel.ComTr)
                {
                    if (Configuration.VisitedHotels.Contains(Hotel.ToDomain()))
                        return;
                    Configuration.VisitedHotels.Add(Hotel.ToDomain());

                    //TODO: unsupported hotel message - Hotel == HHotel.ComTr ? Constants.PHOTO_ISSUE_DIALOG_BODY_TR : Constants.PHOTO_ISSUE_DIALOG_BODY_US
                    // Constants.PHOTO_ISSUE_DIALOG_TITLE
                }
                Master.SaveConfig();
            }
            else if (e.Packet.Id == Out.GetIgnoredUsers) SessionUsername = e.Packet.ReadUTF8(); //TODO: Pull session information with the packet actually meant for it
        }

        public void HandleIncoming(object? sender, DataInterceptedEventArgs e)
        {
            if (HasBeenClosed)
            {
                e.Continue();
                return;
            }

            if (e.Step == 2)
            {
                e.Packet.ReadUTF8();

                IsIncomingEncrypted = e.Packet.ReadBoolean();
                e.Packet.Replace(false, e.Packet.Position - 1);
            }

            if (e.Packet.Id == In.Items) OnItems(e);
            else if (e.Packet.Id == In.FurniList) OnFurniList(e);
            else if (e.Packet.Id == In.ItemDataUpdate) OnItemDataUpdate(e);
            else if (e.Packet.Id == In.RoomReady) OnRoomReady(e);
        }

        private void OnRoomReady(DataInterceptedEventArgs e)
        {
            e.Packet.ReadUTF8();
            CurrentRoomId = e.Packet.ReadInt32();

            _roomPhotoQueue.Clear();
            _roomPhotoItems.Clear();

            _ui.OnPhotoQueueUpdate();
            _ui.UpdateQueueStatus(_roomPhotoQueue.Count);

            if (_isProcessingItems)
            {
                //TODO: Constants.STILL_BUSY_DIALOG_NEWROOM_BODY
            }
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            //TODO: Constants.SCANNING_INVENTORY

            Connection.SendToServerAsync(Out.RequestFurniInventoryWhenNotInRoom);
        }
        private void LoginTitleLbl_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(Constants.BASE_URL + "/register") { UseShellExecute = true });
        }
        private void ExportCertificateBtn_Click(object sender, EventArgs e)
        {
            using var certExportDlg = new FolderBrowserDialog();
            if (certExportDlg.ShowDialog(this) == DialogResult.OK)
            {
                string path = Path.Combine(certExportDlg.SelectedPath, Eavesdropper.Certifier.CertificateAuthorityName + ".cer");

                try
                {
                    Eavesdropper.Certifier.ExportTrustedRootCertificate(path);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(Constants.EXP_CERT_UNAUTH_BODY, Constants.EXP_CERT_UNAUTH_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Connection?.IsConnected ?? false)
            {
                e.Cancel = true;
                ShowInTaskbar = false;

                HasBeenClosed = true;

                Connection.DataIncoming -= HandleIncoming;

                Hide();

                //TODO: Graceful exit logic.
            }
        }

        private void TerminateProxy()
        {
            Eavesdropper.Terminate();
            Eavesdropper.RequestInterceptedAsync -= InjectGameClientAsync;
            Eavesdropper.ResponseInterceptedAsync -= InterceptClientPageAsync;
            Eavesdropper.ResponseInterceptedAsync -= InterceptGameClientAsync;
        }
    }
}