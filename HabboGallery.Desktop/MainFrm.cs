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
using HabboGallery.Desktop.Network;
using HabboGallery.Desktop.Web.Json;
using HabboGallery.Desktop.Utilities;
using HabboGallery.Desktop.Habbo.Camera;
using HabboGallery.Desktop.Habbo.Events;
using HabboGallery.Desktop.Habbo.Network;

using HabboAlerts;

using Sulakore.Habbo;
using Sulakore.Crypto;
using Sulakore.Network;
using Sulakore.Habbo.Messages;

using Eavesdrop;
using System.Text.Json;

#nullable enable
namespace HabboGallery.Desktop
{
    public partial class MainFrm : Form, IMessageFilter
    {
        private const int GET_ITEMDATA_DELAY = 780;
        private const int PUBLISH_QUEUE_POLL_DELAY = 1000;

        public static Program Master => Program.Master;
        
        public static ApiClient Api => Master.Api;
        
        public static Incoming In => Master.In;
        public static Outgoing Out => Master.Out;

        public static HGResources Resources => Master.Resources;
        public static HGConfiguration Configuration => Master.Configuration;

        public static HConnection Connection => Master.Connection;

        private readonly UIUpdater _ui;
        private readonly UdpListener _datagramListener;

        private List<GalleryRecord> _pendingPhotoPurchases;

        //TODO: Lazy loading the images in carousel
        public Dictionary<int, Image> ImageCache { get; }
        public Dictionary<int, GalleryRecord> Photos { get; }

        public int CurrentIndex { get; set; }
        public GalleryRecord? CurrentPhoto => Photos?.Values.ToArray()[CurrentIndex]; //TODO: No lol :D

        private Queue<int> _roomPhotoQueue;
        private Queue<PhotoItem> _photoPublishingQueue;
        private Dictionary<int, HWallItem> _roomPhotoItems;
        
        public int BuyType { get; set; }
        public string? SessionUsername { get; set; }
        public int CurrentRoomId { get; private set; }

        private bool _waitingForPreview;
        private bool _isProcessingItems;

        public bool HasBeenClosed { get; private set; }

        public HHotel Hotel => HotelServer.Hotel;

        public MainFrm()
        {
            Application.AddMessageFilter(this);

            ImageCache = new Dictionary<int, Image>();
            Photos = new Dictionary<int, GalleryRecord>();

            _datagramListener = new UdpListener(Constants.UDP_LISTENER_PORT);
            _datagramListener.BuyRequestReceived += ExternalBuyRequestReceived;

            _ = _datagramListener.ListenAsync(); 

            _pendingPhotoPurchases = new List<GalleryRecord>();
            
            _roomPhotoQueue = new Queue<int>();
            _photoPublishingQueue = new Queue<PhotoItem>();

            _roomPhotoItems = new Dictionary<int, HWallItem>();

            InitializeComponent();

            _ui = new UIUpdater(this);
        }

        public bool PreFilterMessage(ref Message msg)
            => _ui.DragControl(ref msg);

        private async Task DebugAsync(string message) 
        { 
            await Connection.SendToClientAsync(In.Whisper, 0, message, 0, 0, 0, -1).ConfigureAwait(false); 
            Debug.WriteLine(message);
        }

        private async Task CheckForUpdatesAsync()
        {
            double newVersion = await Api.GetLatestVersionAsync();

            if (newVersion > Constants.APP_VERSION)
            {
                var result = MessageBox.Show(Constants.UPDATE_FOUND_BODY, Constants.UPDATE_FOUND_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    Eavesdropper.Terminate();
                    Process.Start(new ProcessStartInfo(Constants.BASE_URL + "/update") { UseShellExecute = true });

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

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble,
                (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
                + (itemCount == 1 ? Constants.SCANNING_SINGLE : Constants.SCANNING_MULTI)
                + Constants.SCANNING_INVENTORY_DONE)
                .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_URL);

            Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

            //TODO: Show user the queueu in photo processing pipeline

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

            IEnumerable<int> unknownIds = await Api
                .BatchCheckExistingIdsAsync(items.Select(i => i.Id), Hotel).ConfigureAwait(false);

            int itemCount = items.Count();
            int unknownCount = unknownIds.Count();

            string alertMessage = (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
                + (itemCount == 0 || itemCount > 1 ? Constants.SCANNING_MULTI : Constants.SCANNING_SINGLE)
                + Constants.SCANNING_WALLITEMS_DONE;

            if (unknownCount > 0)
                alertMessage += " " + unknownCount + Constants.SCANNING_WALLITEMS_UNDISC;

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, alertMessage)
                .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_URL);

            await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog)).ConfigureAwait(false);

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
        private void OnItemDataUpdate(DataInterceptedEventArgs e)
        {
            e.Continue();

            int id = int.Parse(e.Packet.ReadUTF8());
            string extraData = e.Packet.ReadUTF8();

            if (!_roomPhotoItems.ContainsKey(id)) return;

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

                ApiResponse<GalleryRecord> storeResponse = await Api.StorePhotoAsync(photoItem).ConfigureAwait(false);

                if (!ImageCache.ContainsKey(photoItem.Id) &&
                    !Photos.ContainsKey(photoItem.Id) && 
                    storeResponse.TryGetData(out GalleryRecord? record))
                {
                    Photos.Add(photoItem.Id, record);

                    Image photoImage = await Api.GetImageAsync(record.Url, cancellationToken).ConfigureAwait(false);
                    ImageCache.Add(photoItem.Id, record.CreateDateImage(photoImage));

                    _ui.OnPhotoQueueUpdate();
                }
            }
        }
        private Task ProcessPhotoQueueAsync()
        {
            if (_roomPhotoQueue.Count == 1)
            {
                _ = DebugAsync("_roomPhotoQueue.Count == 1 => _isProcessingItems = false;");
                
                _isProcessingItems = false; //Last GetItemData..
            }
            _ = DebugAsync("ProcessPhotoQueueAsync " + _roomPhotoQueue.Peek());

            return Connection.SendToServerAsync(Out.GetItemData, _roomPhotoQueue.Peek());
        }

        private async Task GetKnownPhotoByIdAsync(int photoId)
        {
            //TODO: check imageCache
            ApiResponse<GalleryRecord> photoResponse = await Api.GetPhotoByIdAsync(photoId, Hotel); //TODO: Create batch processing api

            if (!ImageCache.ContainsKey(photoId) && !Photos.ContainsKey(photoId) && photoResponse.TryGetData(out var record))
            {
                Photos.Add(photoId, record);

                Image photoImage = await Api.GetImageAsync(record.Url).ConfigureAwait(false);
                ImageCache.Add(photoId, record.CreateDateImage(photoImage));

                _ui.OnPhotoQueueUpdate();
            }
        }

        public Task SendPhotoAsync(GalleryRecord photo)
        {
            byte[] compressed = Flazzy.Compression.ZLIB.Compress(Encoding.UTF8.GetBytes(PhotoConverter.OldToNew(photo.StrFill, _ui.GetZoomValue()).ToString()));
            return Connection.SendToServerAsync(Out.RenderRoom, compressed.Length, compressed);
        }

        private void BuyPhotoBtn_Click(object sender, EventArgs e)
        {
            _waitingForPreview = true;

            BuyBtn.Enabled = false;
            Resources.RenderButtonState(BuyBtn, BuyBtn.Enabled);

            PublishBtn.Enabled = false;
            Resources.RenderButtonState(PublishBtn, PublishBtn.Enabled);

            IndexDisplayLbl.Text = Constants.PURCHASE_LOADING;
            BuyType = 1;

            _ = SendPhotoAsync(CurrentPhoto);
        }
        private void PublishToWebBtn_Click(object sender, EventArgs e)
        {
            _waitingForPreview = true;

            BuyBtn.Enabled = false;
            Resources.RenderButtonState(BuyBtn, BuyBtn.Enabled);

            PublishBtn.Enabled = false;
            Resources.RenderButtonState(PublishBtn, PublishBtn.Enabled);

            IndexDisplayLbl.Text = Constants.PURCHASE_LOADING;
            BuyType = 2;

            _ = SendPhotoAsync(CurrentPhoto);
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
            Configuration.Save();

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
                        Configuration.Save();
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

        public void HandleOutgoing(object? sender, DataInterceptedEventArgs e)
        {
            if (HasBeenClosed)
            {
                e.Continue();
                if (e.Packet.Id == Out.GetExtendedProfileByName)
                {
                    var eventResponse = JsonSerializer.Deserialize<EventResponse>(e.Packet.ReadUTF8());

                    if (eventResponse?.Name == Constants.RESPONSE_NAME_CLOSE_FORM)
                        Connection.Disconnect();
                }
                return;
            }

            if (e.Packet.Id == 4001)
            {
                string sharedKeyHex = e.Packet.ReadUTF8();
                if (sharedKeyHex.Length % 2 != 0)
                {
                    sharedKeyHex = "0" + sharedKeyHex;
                }

                byte[] sharedKey = Enumerable.Range(0, sharedKeyHex.Length / 2)
                    .Select(x => Convert.ToByte(sharedKeyHex.Substring(x * 2, 2), 16))
                    .ToArray();

                Connection.Remote.Encrypter = new RC4(sharedKey);
                Connection.Remote.IsEncrypting = true;

                if (IsIncomingEncrypted)
                {
                    Connection.Remote.Decrypter = new RC4(sharedKey);
                    Connection.Remote.IsDecrypting = true;
                }

                e.IsBlocked = true;
            }
            else if (e.Packet.Id == Out.InfoRetrieve) //TODO: Clean up init
            {
                //TODO: CancellationTokenSauces fired here?
                _ = HandlePhotoQueueAsync();
                _ = ProcessPhotoItemsAsync();

                _ = _datagramListener.ListenAsync(); //TODO:

                if (Configuration.IsFirstConnect)
                {
                    Configuration.IsFirstConnect = false;

                    HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, Constants.INTRO_DIALOG_BODY)
                        .WithTitle(Constants.INTRO_DIALOG_TITLE);

                    Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }
                else
                {
                    HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.APP_CONNECT_SUCCESS)
                        .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_URL);

                    Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }

                if (Hotel == HHotel.Com || Hotel == HHotel.ComTr)
                {
                    if (Configuration.VisitedHotels.Contains(Hotel.ToDomain()))
                        return;
                    Configuration.VisitedHotels.Add(Hotel.ToDomain());

                    HabboAlert countryIssueAlert = AlertBuilder.CreateAlert(HabboAlertType.PopUp,
                        Hotel == HHotel.ComTr ? Constants.PHOTO_ISSUE_DIALOG_BODY_TR : Constants.PHOTO_ISSUE_DIALOG_BODY_US)
                        .WithTitle(Constants.PHOTO_ISSUE_DIALOG_TITLE)
                        .WithEventTitle(Constants.PHOTO_ISSUE_DIALOG_EVENT_TITLE)
                        .WithEventUrl(string.Empty)
                        .WithImageUrl(Constants.PHOTO_ISSUE_DIALOG_IMAGE_URL);

                    Connection.SendToClientAsync(countryIssueAlert.ToPacket(In.NotificationDialog));
                }
                Configuration.Save();
            }
            else if (e.Packet.Id == Out.GetExtendedProfileByName)
            {
                string responseProfileName = e.Packet.ReadUTF8();
                EventResponse? response = JsonSerializer.Deserialize<EventResponse>(responseProfileName);
                switch (response?.Name)
                {
                    case Constants.RESPONSE_NAME_EXTERNAL_BUY:
                        {
                            GalleryRecord? photo = _pendingPhotoPurchases.FirstOrDefault(p => IdentifierFromUrl(p.Url) == response.Data);
                            if (photo != null)
                            {
                                BuyType = 1;
                                _waitingForPreview = true;
                                
                                SendPhotoAsync(photo);
                            }
                            break;
                        }
                    case Constants.RESPONSE_NAME_CLOSE_FORM:
                        Connection.Disconnect();
                        break;
                }
            }
            else if (e.Packet.Id == Out.GetIgnoredUsers) SessionUsername = e.Packet.ReadUTF8(); //TODO: Pull session information with the packet actually meant for it
            else if (e.Packet.Id == Out.UseWallItem) OnUseWallItem(e);
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
            else if (e.Packet.Id == In.CameraStorageUrl) OnPhotoStorageUrl(e);
            else if (e.Packet.Id == In.RoomReady) OnRoomReady(e);
            /*
             * This blocks an informative message about friend finding rooms.
             * We're abusing the mechanic, so we don't want to see the message! :-)
             */
            else if (e.Packet.Id == In.FriendFindingRoom) e.IsBlocked = true;
        }
        
        public void OnUseWallItem(DataInterceptedEventArgs e)
        {
            int furniId = e.Packet.ReadInt32();
            e.Packet.ReadInt32();

            if (ImageCache.ContainsKey(furniId))
            {
                CurrentIndex = Photos.Values.ToList().FindIndex(p => p.Id == furniId);
                _ui.OnPhotoQueueUpdate();
            }
        }

        private void OnPhotoStorageUrl(DataInterceptedEventArgs e)
        {
            if (!_waitingForPreview) return;

            Connection.SendToServerAsync(BuyType == 1 ? Out.PurchasePhoto : Out.PublishPhoto);

            _ui.OnPhotoQueueUpdate();
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
                HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_NEWROOM_BODY)
                    .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_URL);

                Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
            }
        }

        private string IdentifierFromUrl(string url) => url[24..].Split('.')[0];

        private void ExternalBuyRequestReceived(object? sender, GalleryRecord e)
        {
            _pendingPhotoPurchases.Add(e);

            string paddedAlertTitle = Constants.EXTERNAL_BUY_DIALOG_EVENT_TITLE
                .PadLeft(Constants.EXTERNAL_BUY_DIALOG_SPACE_COUNT +3 )
                .PadRight(Constants.EXTERNAL_BUY_DIALOG_SPACE_COUNT * 2+3);

            string identifier = IdentifierFromUrl(e.Url); //TODO: ew

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, Constants.EXTERNAL_BUY_DIALOG_BODY)
                .WithEventTitle(paddedAlertTitle)
                .WithTitle(Constants.EXTERNAL_BUY_DIALOG_TITLE)
                .WithImageUrl("https:" + e.Url)
                .WithEventUrl(new EventResponse(Constants.RESPONSE_NAME_EXTERNAL_BUY, identifier).ToEventString());

            Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.SCANNING_INVENTORY)
                   .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_URL);

            Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
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
                _datagramListener.BuyRequestReceived -= ExternalBuyRequestReceived;

                Hide();

                HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.FORM_CLOSED_DIALOG_BODY)
                    .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_URL)
                    .WithEventUrl(new EventResponse(Constants.RESPONSE_NAME_CLOSE_FORM, string.Empty).ToEventString());

                Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
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