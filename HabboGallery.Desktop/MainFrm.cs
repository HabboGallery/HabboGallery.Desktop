using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Net.Http;
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
using HabboGallery.Desktop.Habbo.Guidance;

using HabboAlerts;

using Sulakore.Habbo;
using Sulakore.Crypto;
using Sulakore.Network;
using Sulakore.Habbo.Web;
using Sulakore.Habbo.Messages;
using Sulakore.Network.Protocol;

using Flazzy;

using Eavesdrop;

namespace HabboGallery.Desktop
{
    public partial class MainFrm : Form, IMessageFilter
    {
        public Program Master => Program.Master;

        public ApiClient ApiClient => Master.ApiClient;

        public HGResources Resources => Master.Resources;
        public HGConfiguration Configuration => Master.Configuration;

        public HHotel Hotel => Master.GameData.Hotel;
        public HConnection Connection => Master.Connection;

        private readonly UIUpdater _ui;
        
        private Guid _randomQuery;
        
        private UdpListener _datagramListener;

        private bool _waitingForPreview;
        private bool _waitingForPostItData;
        private bool _inventoryItemsRequested;

        public Incoming In { get; set; }
        public Outgoing Out { get; set; }

        public Dictionary<int, OldPhoto> Photos { get; set; }
        public Dictionary<int, Image> ImageCache { get; set; }

        public List<int> RoomItemsQueue { get; set; }

        public Dictionary<int, (HWallItem Item, int RoomId)> ExtraPhotoData { get; }

        public List<OldPhoto> UnconfirmedExternalBuyRequests { get; set; }

        public NewUserTour Tour { get; set; }

        public OldPhoto CurrentPhoto => Photos?.Values.ToArray()[CurrentIndex];

        public int CurrentIndex { get; set; }
        public int BuyType { get; set; }
        public bool TourRunning { get; set; }
        public string UsernameOfSession { get; set; }
        public int CurrentRoomId { get; private set; }
        
        public PhotoLoadType LoadingPhotos { get; set; }
        
        public bool HasBeenClosed { get; private set; }

        public bool IsIncomingEncrypted { get; private set; }
        public HotelEndPoint HotelServer { get; set; }

        public MainFrm()
        {
            Photos = new Dictionary<int, OldPhoto>();
            
            ImageCache = new Dictionary<int, Image>();

            RoomItemsQueue = new List<int>();
            ExtraPhotoData = new Dictionary<int, (HWallItem, int)>();
            UnconfirmedExternalBuyRequests = new List<OldPhoto>();

            InitializeComponent();
            
            _ui = new UIUpdater(this);
        }

        public bool PreFilterMessage(ref Message m) 
            => _ui.DragControl(ref m);

        private async Task AfterLoginAsync()
        {
            await HandleQueueAsync().ConfigureAwait(false); //TODO:
        }

        private async Task CheckForUpdatesAsync()
        {
            double newVersion = await ApiClient.GetLatestVersionAsync();

            if (newVersion > Constants.APP_VERSION)
            {
                var result = MessageBox.Show(Constants.UPDATE_FOUND_BODY, Constants.UPDATE_FOUND_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    Eavesdropper.Terminate();
                    Process.Start(new ProcessStartInfo(Constants.BASE_URL + Constants.UPDATE_URL) { UseShellExecute = true });

                    Environment.Exit(0);
                }
            }
            else _ui.SetStatusMessage(Constants.UP_TO_DATE_MESSAGE);
        }
        private async Task HandleQueueAsync(CancellationToken cancellationToken = default) //TODO: Rewrite whole "event loop"
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(780);

                _waitingForPostItData = false;

                if (RoomItemsQueue.Count > 0)
                    ProgressRoomItemsQueue();
            }
        }

        private void ConnectionClosed(object sender, EventArgs e)
        {
            _ui.SetStatusMessage(Constants.DISCONNECTED);
            
            SearchBtn.Enabled = false;
            Resources.RenderButtonState(SearchBtn, SearchBtn.Enabled);

            Environment.Exit(0);
        }
        private void ConnectionOpened(object sender, ConnectedEventArgs e)
        {
            HPacket endPointPkt = Connection.Local.ReceivePacketAsync().Result;
            string host = endPointPkt.ReadUTF8();
            int port = endPointPkt.ReadInt32();
            e.HotelServer = HotelServer = HotelEndPoint.Parse(host, port);

            _ui.SetStatusMessage(Constants.CONNECTED);

            SearchBtn.Enabled = true;
            Resources.RenderButtonState(SearchBtn, SearchBtn.Enabled);
        }

        private void OnPreviewLoaded(DataInterceptedEventArgs e)
        {
            if (!_waitingForPreview) return;

            Connection.SendToServerAsync(BuyType == 1 ? Out.PurchasePhoto : Out.PublishPhoto);
            _ui.OnPhotoQueueUpdate();
        }
        private void OnInventoryLoaded(DataInterceptedEventArgs e)
        {
            e.Continue();

            LoadingPhotos = PhotoLoadType.Inventory;

            IEnumerable<CHItem> items =
                CHItem.Parse(e.Packet).Where(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id));

            int itemCount = items.Count();

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble,
                (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
                + (itemCount == 1 ? Constants.SCANNING_SINGLE : Constants.SCANNING_MULTI)
                + Constants.SCANNING_INVENTORY_DONE)
                .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

            Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

            int queueCounter = 0;

            foreach (CHItem item in items)
            {
                _ui.UpdateQueueStatus(itemCount - queueCounter);

                ProcessPhotoDataAsync(item.Id, item.ExtraData, UsernameOfSession);

                queueCounter++;
            }

            _ui.UpdateQueueStatus(itemCount - queueCounter);
            LoadingPhotos = PhotoLoadType.None;
        }

        private async void OnItemData(DataInterceptedEventArgs e)
        {
            int id = int.Parse(e.Packet.ReadUTF8());
            string extraData = e.Packet.ReadUTF8();

            try
            {
                if (OldPhoto.Validate(extraData) && !Photos.ContainsKey(id))
                {
                    string ownerName = ExtraPhotoData.ContainsKey(id) ? ExtraPhotoData[id].Item.OwnerName : null;
                    int roomId = ExtraPhotoData.ContainsKey(id) ? ExtraPhotoData[id].RoomId : 0;

                    await ProcessPhotoDataAsync(id, extraData, ownerName, roomId);
                }
            }
            catch (FormatException) { }

            if (_waitingForPostItData && RoomItemsQueue.First() == id)
            {
                _waitingForPostItData = false;
                RoomItemsQueue.RemoveAt(0);

                _ui.UpdateQueueStatus(RoomItemsQueue.Count);
                ProgressRoomItemsQueue();
            }
        }

        private async void OnWallItems(DataInterceptedEventArgs e)
        {
            e.Continue();

            LoadingPhotos = PhotoLoadType.Room;

            List<HWallItem> items = HWallItem.Parse(e.Packet).ToList().FindAll(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id) && !RoomItemsQueue.Contains(i.Id) && !ExtraPhotoData.ContainsKey(i.Id));
            int[] itemIds = await ApiClient.BatchCheckExistingIdsAsync(items.Select(i => i.Id).ToArray(), Hotel);

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble,
                (items.Count == 0 ? Constants.SCANNING_EMPTY : items.Count.ToString())
                + (items.Count == 0 || items.Count > 1 ? Constants.SCANNING_MULTI : Constants.SCANNING_SINGLE)
                + Constants.SCANNING_WALLITEMS_DONE + itemIds.Length + Constants.SCANNING_WALLITEMS_UNDISC)
                .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

            await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

            foreach (HWallItem item in items)
            {
                if (itemIds.Contains(item.Id))
                {
                    RoomItemsQueue.Add(item.Id);
                    ExtraPhotoData.Add(item.Id, (item, CurrentRoomId));
                }
                else
                {
                    await GetKnownPhotoByIdAsync(item.Id);
                }
            }

            ProgressRoomItemsQueue();
        }

        private async Task ProcessPhotoDataAsync(int id, string extradata, string ownerName, int roomId = 0)
        {
            OldPhoto photo = OldPhoto.Parse(id, extradata, Hotel);

            ApiResponse<OldPhoto> photoPublishResponse = await ApiClient.PublishPhotoDataAsync(photo, ownerName, roomId);

            if (!ImageCache.ContainsKey(id) && !Photos.ContainsKey(id) && photoPublishResponse.Success)
            {
                Photos.Add(id, photoPublishResponse.Data);
                ImageCache.Add(id, photoPublishResponse.Data.CreateDateImage(await ApiClient.GetImageAsync(new Uri(photoPublishResponse.Data.Url))));
                
                _ui.OnPhotoQueueUpdate();
            }
        }
        private async Task GetKnownPhotoByIdAsync(int photoId)
        {
            ApiResponse<OldPhoto> photoResponse = await ApiClient.GetPhotoByIdAsync(photoId, Hotel);

            if (!ImageCache.ContainsKey(photoId) && !Photos.ContainsKey(photoId) && photoResponse.Success)
            {
                Photos.Add(photoId, photoResponse.Data);
                ImageCache.Add(photoId, photoResponse.Data.CreateDateImage(await ApiClient.GetImageAsync(new Uri(photoResponse.Data.Url))));
                
                _ui.OnPhotoQueueUpdate();
            }
        }

        private async void ProgressRoomItemsQueue()
        {
            if (!_waitingForPostItData && RoomItemsQueue.Count > 0)
            {
                _waitingForPostItData = true;
                await Connection.SendToServerAsync(Out.GetItemData, RoomItemsQueue.First());
            }
            else if (RoomItemsQueue.Count == 0)
            {
                _ui.UpdateQueueStatus(RoomItemsQueue.Count);

                LoadingPhotos = PhotoLoadType.None;
            }
        }

        public async Task SendPhotoAsync(OldPhoto photo)
        {
            byte[] compressed = Flazzy.Compression.ZLIB.Compress(Encoding.UTF8.GetBytes(PhotoConverter.OldToNew(photo, _ui.GetZoomValue()).ToString()));
            await Connection.SendToServerAsync(Out.RenderRoom, compressed.Length, compressed);
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

            SendPhotoAsync(CurrentPhoto);
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

        private void PublishToWebBtn_Click(object sender, EventArgs e)
        {
            _waitingForPreview = true;

            BuyBtn.Enabled = false;
            Resources.RenderButtonState(BuyBtn, BuyBtn.Enabled);

            PublishBtn.Enabled = false;
            Resources.RenderButtonState(PublishBtn, PublishBtn.Enabled);

            IndexDisplayLbl.Text = Constants.PURCHASE_LOADING;
            BuyType = 2;

            SendPhotoAsync(CurrentPhoto);
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
            TerminateProxy();
            Close();
        }

        private async void LoginSubmitBtn_Click(object sender, EventArgs e)
        {
            _ui.SetLoginMessage(string.Empty, false);
            try
            {
                if (await ApiClient.LoginAsync(LoginEmailTxt.Text, LoginPasswordTxt.Text))
                {
                    Configuration.Email = LoginEmailTxt.Text;
                    if (RememberMeBx.Checked)
                    {
                        Configuration.Password = LoginPasswordTxt.Text;
                    }

                    _ui.HideLogin();
                    await AfterLoginAsync().ConfigureAwait(false);
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
            
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            if (Configuration.RememberMe)
            {
                LoginEmailTxt.Text = Configuration.Email;
                LoginPasswordTxt.Text = Configuration.Password;
                RememberMeBx.Checked = true;
            }
        }

        private void RememberMeBx_CheckedChanged(object sender, EventArgs e)
        {
            Configuration.RememberMe = RememberMeBx.Checked;
        }

        private Task InjectGameClientAsync(object sender, RequestInterceptedEventArgs e)
        {
            if (!e.Uri.Query.StartsWith("?" + _randomQuery)) return Task.CompletedTask;

            Eavesdropper.RequestInterceptedAsync -= InjectGameClientAsync;

            Uri remoteUrl = e.Request.RequestUri;

            string clientPath = Path.Combine(Master.DataDirectory.FullName,
                $@"{Constants.MODDED_CLIENTS_FOLDER_NAME}\{remoteUrl.Host}\{remoteUrl.LocalPath}");

            if (!File.Exists(clientPath))
            {
                _ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT);
                Eavesdropper.ResponseInterceptedAsync += InterceptGameClientAsync;
            }
            else
            {
                _ui.SetStatusMessage(Constants.DISASSEMBLING_CLIENT);
                using var game = new HGame(clientPath);
                game.Disassemble();

                _ui.SetStatusMessage(Constants.GENERATING_MESSAGE_HASHES);
                game.GenerateMessageHashes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.HASHES_FILE_NAME));

                In = game.In;
                Out = game.Out;

                InterceptConnectionAsync().ConfigureAwait(false);

                e.Request = WebRequest.Create(new Uri(clientPath));
                TerminateProxy();
            }

            return Task.CompletedTask;
        }
        private async Task InterceptGameClientAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (!e.Uri.Query.StartsWith("?" + _randomQuery)) return;
            if (e.ContentType != Constants.CONTENT_TYPE_FLASH) return;
            Eavesdropper.ResponseInterceptedAsync -= InterceptGameClientAsync;

            string clientPath = Path.Combine(Master.DataDirectory.FullName, $@"{Constants.MODDED_CLIENTS_FOLDER_NAME}\{e.Uri.Host}\{e.Uri.LocalPath}"); ;
            string clientDirectory = Path.GetDirectoryName(clientPath);
            Directory.CreateDirectory(clientDirectory);

            _ui.SetStatusMessage(Constants.DISASSEMBLING_CLIENT);
            using var game = new HGame(await e.Content.ReadAsStreamAsync().ConfigureAwait(false));
            game.Location = clientPath;
            
            game.Disassemble();

            _ui.SetStatusMessage(Constants.GENERATING_MESSAGE_HASHES);
            game.GenerateMessageHashes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.HASHES_FILE_NAME));

            In = game.In;
            Out = game.Out;

            _ui.SetStatusMessage(Constants.MODIFYING_CLIENT);
            game.DisableHostChecks();
            game.InjectKeyShouter(4001);
            game.InjectEndPointShouter(4000);
            game.InjectEndPoint("127.0.0.1", Connection.ListenPort);

            CompressionKind compression = CompressionKind.ZLIB;
#if DEBUG
            compression = CompressionKind.None;
#endif

            _ui.SetStatusMessage(Constants.ASSEMBLING_CLIENT);
            byte[] payload = game.ToArray(compression);
            e.Headers[HttpResponseHeader.ContentLength] = payload.Length.ToString();

            e.Content = new ByteArrayContent(payload);
            using (FileStream clientStream = File.Open(clientPath, FileMode.Create, FileAccess.Write))
            {
                clientStream.Write(payload);
            }

            TerminateProxy();
            await InterceptConnectionAsync().ConfigureAwait(false);
        }
        private async Task InterceptClientPageAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (e.Content == null) return;
            if (!e.ContentType.Contains(Constants.CONTENT_TYPE_TEXT)) return;

            string body = await e.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!body.Contains("info.host") && !body.Contains("info.port")) return;

            Eavesdropper.ResponseInterceptedAsync -= InterceptClientPageAsync;
            Master.GameData.Source = body;

            body = body.Replace(".swf", $".swf?{_randomQuery = Guid.NewGuid()}");
            e.Content = new StringContent(body);

            _ui.SetStatusMessage(Constants.INJECTING_CLIENT);
            Eavesdropper.RequestInterceptedAsync += InjectGameClientAsync;
        }

        private void TerminateProxy()
        {
            Eavesdropper.Terminate();
            Eavesdropper.RequestInterceptedAsync -= InjectGameClientAsync;
            Eavesdropper.ResponseInterceptedAsync -= InterceptClientPageAsync;
            Eavesdropper.ResponseInterceptedAsync -= InterceptGameClientAsync;
        }

        private async Task InterceptConnectionAsync()
        {
            _ui.SetStatusMessage(Constants.INTERCEPTING_CONNECTION);
            await Connection.InterceptAsync(HotelServer).ConfigureAwait(false);
        }


        public async void HandleOutgoing(object sender, DataInterceptedEventArgs e)
        {
            if (HasBeenClosed)
            {
                e.Continue();
                if (e.Packet.Id == Out.GetUserProfileByName)
                {
                    var eventResponse = EventResponse.Parse(e.Packet);

                    if (eventResponse.Name == Constants.RESPONSE_NAME_CLOSE_FORM)
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
                _datagramListener = new UdpListener(Constants.UDP_LISTENER_PORT);
                _datagramListener.BuyRequestReceived += ExternalBuyRequestReceived;
                 _ = _datagramListener.ListenAsync(); //TODO:

                if (Configuration.IsFirstConnect)
                {
                    Configuration.IsFirstConnect = true;

                    TourRunning = true;
                
                    Tour = new NewUserTour(Connection, _ui, In.NotificationDialog, In.InClientLink);
                    await Tour.ShowIntroDialogAsync();
                }
                else
                {
                    HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.APP_CONNECT_SUCCESS)
                        .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);
                
                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }

                if (Hotel == HHotel.Com || Hotel == HHotel.ComTr)
                {
                    if (Configuration.VisitedHotels.Contains(Hotel.ToDomain())) return;
                    Configuration.VisitedHotels.Add(Hotel.ToDomain());

                    HabboAlert countryIssueAlert = AlertBuilder.CreateAlert(HabboAlertType.PopUp,
                        Hotel == HHotel.ComTr ? Constants.PHOTO_ISSUE_DIALOG_BODY_TR : Constants.PHOTO_ISSUE_DIALOG_BODY_US)
                        .WithTitle(Constants.PHOTO_ISSUE_DIALOG_TITLE)
                        .WithEventTitle(Constants.PHOTO_ISSUE_DIALOG_EVENT_TITLE)
                        .WithEventUrl(string.Empty)
                        .WithImageUrl(Constants.PHOTO_ISSUE_DIALOG_IMAGE_URL);

                    await Connection.SendToClientAsync(countryIssueAlert.ToPacket(In.NotificationDialog));
                }
            }
            else if (e.Packet.Id == Out.FindNewFriends && TourRunning)
            {
                e.Continue();

                await Task.Delay(3000);
                await Tour.ShowGuideStartedMessageAsync();
            }
            else if (e.Packet.Id == Out.NuxScriptProceed && TourRunning)
            {
                e.IsBlocked = true;
                await Tour.ShowNextHelpBubbleAsync();
            }
            else if (e.Packet.Id == Out.GetUserProfileByName)
            {
                EventResponse response = EventResponse.Parse(e.Packet);
                switch (response.Name)
                {
                    case Constants.RESPONSE_NAME_NUT:
                        await Tour.ShowNextHelpBubbleAsync();
                        break;
                    case Constants.RESPONSE_NAME_EXTERNAL_BUY:
                        {
                            OldPhoto photo = UnconfirmedExternalBuyRequests.Find(p => IdentifierFromUrl(p.Url) == response.Data);
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
            else if (e.Packet.Id == Out.Username && string.IsNullOrWhiteSpace(UsernameOfSession))
            {
                UsernameOfSession = e.Packet.ReadUTF8();
            }
            else if (e.Packet.Id == Out.UseWallItem)
            {
                int furniId = e.Packet.ReadInt32();
                e.Packet.ReadInt32();

                if (ImageCache.ContainsKey(furniId))
                {
                    CurrentIndex = Photos.Values.ToList().FindIndex(p => p.Id == furniId);
                    _ui.OnPhotoQueueUpdate();
                }
            }
        }

        public string IdentifierFromUrl(string url) 
            => url.Remove(0, 24).Split('.')[0];

        private async void ExternalBuyRequestReceived(object sender, OldPhoto e)
        {
            if (HasBeenClosed)
                return;

            UnconfirmedExternalBuyRequests.Add(e);

            string paddedAlertTitle = Constants.EXTERNAL_BUY_DIALOG_EVENT_TITLE
                .PadLeft(Constants.EXTERNAL_BUY_DIALOG_SPACE_COUNT)
                .PadRight(Constants.EXTERNAL_BUY_DIALOG_SPACE_COUNT);

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, Constants.EXTERNAL_BUY_DIALOG_BODY)
                .WithEventTitle(paddedAlertTitle)
                .WithTitle(Constants.EXTERNAL_BUY_DIALOG_TITLE)
                .WithImageUrl("https:" + e.Url)
                .WithEventUrl(new EventResponse(Constants.RESPONSE_NAME_EXTERNAL_BUY, IdentifierFromUrl(e.Url)).ToEventString());

            await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
        }

        public async void HandleIncoming(object sender, DataInterceptedEventArgs e)
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
            if (e.Packet.Id == In.InventoryItems && _inventoryItemsRequested)
            {
                _inventoryItemsRequested = false;
                OnInventoryLoaded(e);
            }
            else if (e.Packet.Id == In.CameraStorageUrl)
            {
                OnPreviewLoaded(e);
            }
            else if (e.Packet.Id == In.ItemDataUpdate)
            {
                OnItemData(e);
            }
            else if (e.Packet.Id == In.Items)
            {
                if (LoadingPhotos == PhotoLoadType.None)
                {
                    OnWallItems(e);
                }
                else if (LoadingPhotos == PhotoLoadType.Inventory)
                {
                    HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_INV_BODY)
                            .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }
                else
                {
                    foreach (int id in RoomItemsQueue)
                    {
                        Photos.Remove(id);
                        ExtraPhotoData.Remove(id);
                    }

                    RoomItemsQueue.Clear();
                    
                    _ui.OnPhotoQueueUpdate();
                    _ui.UpdateQueueStatus(RoomItemsQueue.Count);

                    _waitingForPostItData = false;

                    HabboAlert alert =
                        AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_NEWROOM_BODY)
                            .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

                    OnWallItems(e);
                }
            }
            else if (e.Packet.Id == In.FriendFindingRoom)
            {
                /*
                 * This blocks an informative message about friend finding rooms.
                 * We're abusing the mechanic, so we don't want to see the message! :-)
                 */
                e.IsBlocked = true;
            }
            else if (e.Packet.Id == In.RoomReady)
            {
                e.Packet.ReadUTF8();
                CurrentRoomId = e.Packet.ReadInt32();
            }
        }

        private async void SearchBtn_Click(object sender, EventArgs e)
        {
            if (LoadingPhotos != PhotoLoadType.Room)
            {
                _inventoryItemsRequested = true;

                HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.SCANNING_INVENTORY)
                    .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                await Connection.SendToServerAsync(Out.TestInventory);
            }
            else
            {
                HabboAlert alert =
                    AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_ROOM_BODY)
                        .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
            }
        }

        private void LoginTitleLbl_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(Constants.BASE_URL + "register") { UseShellExecute = true });
        }

        private async void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Connection != null && Connection.IsConnected)
            {
                e.Cancel = true;
                ShowInTaskbar = false;
                HasBeenClosed = true;
                Hide();

                HabboAlert alert =
                    AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.FORM_CLOSED_DIALOG_BODY)
                        .WithImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH)
                        .WithEventUrl(new EventResponse(Constants.RESPONSE_NAME_CLOSE_FORM, string.Empty).ToEventString());

                await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
            }
        }

        private void ExportCertificateBtn_Click(object sender, EventArgs e)
        {
            using var certExportDlg = new FolderBrowserDialog();

            if (certExportDlg.ShowDialog(this) == DialogResult.OK)
            {
                string path = certExportDlg.SelectedPath;

                try
                {
                    Eavesdropper.Certifier.ExportTrustedRootCertificate(Path.Combine(path, Eavesdropper.Certifier.CertificateAuthorityName + ".cer"));
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(Constants.EXP_CERT_UNAUTH_BODY, Constants.EXP_CERT_UNAUTH_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}