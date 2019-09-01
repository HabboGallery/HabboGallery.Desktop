using Eavesdrop;
using Flazzy;
using HabboAlerts;
using HabboGallery.Communication;
using HabboGallery.Habbo;
using HabboGallery.Habbo.Camera;
using HabboGallery.Habbo.Events;
using HabboGallery.Habbo.Guidance;
using HabboGallery.Habbo.Network;
using HabboGallery.Properties;
using HabboGallery.UI;
using HabboGallery.Web;
using HabboGallery.Web.Json;
using Sulakore.Crypto;
using Sulakore.Habbo;
using Sulakore.Habbo.Messages;
using Sulakore.Habbo.Web;
using Sulakore.Network;
using Sulakore.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HabboGallery
{
    public partial class MainFrm : Form, IMessageFilter
    {
        public ApiClient ApiClient { get; private set; }
        private Guid _randomQuery;
        private readonly UIUpdater _ui;
        private DatagramListener _datagramListener;

        private bool _waitingForPreview;
        private bool _waitingForPostItData;

        public HGame Game { get; set; }
        public HGameData GameData { get; set; }
        public HConnection Connection { get; set; }

        public Incoming In { get; set; }
        public Outgoing Out { get; set; }

        public Dictionary<int, OldPhoto> Photos { get; set; }
        public Dictionary<int, Image> ImageCache { get; set; }
        public List<int> RoomItemsQueue { get; set; }
        public Dictionary<int, (HWallItem Item, int RoomId)> ExtraPhotoData { get; }
        public List<OldPhoto> UnconfirmedExternalBuyRequests { get; set; }

        public NewUserTour Tour { get; set; }

        private bool _inventoryItemsRequested;

        public string AppDataPath { get; set; }

        public OldPhoto CurrentPhoto
            => Photos?.Values.ToArray()[CurrentIndex];

        public int CurrentIndex { get; set; }
        public int BuyType { get; set; }
        public bool TourRunning { get; set; }
        public string UsernameOfSession { get; set; }
        public int CurrentRoomId { get; private set; }
        public PhotoLoadType LoadingPhotos { get; set; }

        public int CurrentRoomTotalPhotosFound { get; set; }
        public int CurrentRoomPhotosSucceeded { get; set; }
        public bool CurrentRoomUserShouldBeNotified { get; set; }

        public MainFrm()
        {
            ImageCache = new Dictionary<int, Image>();
            Photos = new Dictionary<int, OldPhoto>();
            RoomItemsQueue = new List<int>();
            ExtraPhotoData = new Dictionary<int, (HWallItem, int)>();
            UnconfirmedExternalBuyRequests = new List<OldPhoto>();
            TourRunning = false;

            ApiClient = new ApiClient(new Uri(Constants.BASE_URL));
            InitializeComponent();
            _ui = new UIUpdater(this);
            AppDataPath = SetAppData();
        }

        private string SetAppData()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APP_NAME);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private async Task AfterLogin()
        {
            Eavesdropper.Certifier = new CertificateManager(Constants.APP_NAME, Constants.CERT_CERTIFICATE_NAME);

            GameData = new HGameData();
            Connection = new HConnection();
            Connection.DataOutgoing += HandleOutgoing;
            Connection.DataIncoming += HandleIncoming;

            Connection.Connected += ConnectionOpened;
            Connection.Disconnected += ConnectionClosed;

            In = new Incoming();
            Out = new Outgoing();

            if (Eavesdropper.Certifier.CreateTrustedRootCertificate())
            {
                Eavesdropper.ResponseInterceptedAsync += InterceptClientPageAsync;
                Eavesdropper.Initiate(Constants.PROXY_PORT);

                _ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT_PAGE);
            }

            await HandleQueueAsync().ConfigureAwait(false);
        }

        private async Task HandleQueueAsync()
        {
            while (true)
            {
                await Task.Delay(1000);
                _waitingForPostItData = false;
                if (RoomItemsQueue.Count > 0)
                    ProgressRoomItemsQueue();
            }
        }

        private void ConnectionClosed(object sender, EventArgs e)
        {
            _ui.SetStatusMessage(Constants.DISCONNECTED);
            _ui.ToggleSearchButton(false);
            Environment.Exit(0);
        }

        private void ConnectionOpened(object sender, ConnectedEventArgs e)
        {
            HPacket endPointPkt = Connection.Local.ReceivePacketAsync().Result;
            string host = endPointPkt.ReadUTF8();
            int port = endPointPkt.ReadInt32();
            e.HotelServer = HotelServer = HotelEndPoint.Parse(host, port);

            _ui.SetStatusMessage(Constants.CONNECTED);
            _ui.ToggleSearchButton(true);
        }

        private async void OnPreviewLoaded(DataInterceptedEventArgs e)
        {
            if (!_waitingForPreview) return;

            await Connection.SendToServerAsync(BuyType == 1 ? Out.CameraPurchase : Out.CameraPublishToWeb);
            _ui.Update();
        }

        private async void OnInventoryLoaded(DataInterceptedEventArgs e)
        {
            e.Continue();

            LoadingPhotos = PhotoLoadType.Inventory;

            List<CHItem> items = CHItem.Parse(e.Packet).ToList().FindAll(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id));

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble,
                (items.Count == 0 ? Constants.SCANNING_EMPTY : items.Count.ToString())
                + (items.Count == 0 || items.Count > 1 ? Constants.SCANNING_MULTI : Constants.SCANNING_SINGLE)
                + Constants.SCANNING_INVENTORY_DONE)
                .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

            await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

            int queueCounter = 0;

            foreach (CHItem item in items)
            {
                _ui.UpdateInventoryQueueStatusMessage(items.Count - queueCounter);
                await ProcessPhotoDataAsync(item.Id, item.ExtraData, UsernameOfSession);
                queueCounter++;
            }

            _ui.UpdateInventoryQueueStatusMessage(items.Count - queueCounter);
            LoadingPhotos = PhotoLoadType.None;
        }
        private async void OnPostItDataReceived(DataInterceptedEventArgs e)
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
                _ui.UpdateRoomItemsQueueStatusMessage();
                ProgressRoomItemsQueue();
            }
        }

        private async Task ProcessPhotoDataAsync(int id, string extradata, string ownerName, int roomId = 0)
        {
            OldPhoto photo = OldPhoto.Parse(id, extradata, GameData.Hotel);

            ApiResponse<OldPhoto> photoPublishResponse = await ApiClient.PublishPhotoDataAsync(photo, ownerName, roomId);

            if (!ImageCache.ContainsKey(id) && !Photos.ContainsKey(id) && photoPublishResponse.Success)
            {
                Photos.Add(id, photo);
                Photos[id].StrFill = photoPublishResponse.Data.StrFill;
                ImageCache.Add(id, photoPublishResponse.Data.CreateDateImage(await ApiClient.GetPhotoAsync(new Uri(photoPublishResponse.Data.Url))));
                _ui.Update();
            }

            CurrentRoomPhotosSucceeded += photoPublishResponse.Success ? 1 : 0;
        }

        private async void RoomWallItemsLoaded(DataInterceptedEventArgs e)
        {
            e.Continue();

            LoadingPhotos = PhotoLoadType.Room;

            List<HWallItem> items = HWallItem.Parse(e.Packet).ToList().FindAll(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id) && !RoomItemsQueue.Contains(i.Id) && !ExtraPhotoData.ContainsKey(i.Id));
            int[] itemIds = items.Select(i => i.TypeId).ToArray();

            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble,
                (items.Count == 0 ? Constants.SCANNING_EMPTY : items.Count.ToString())
                + (items.Count == 0 || items.Count > 1 ? Constants.SCANNING_MULTI : Constants.SCANNING_SINGLE)
                + Constants.SCANNING_WALLITEMS_DONE)
                .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

            await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

            foreach (HWallItem item in items)
            {
                RoomItemsQueue.Add(item.Id);
                ExtraPhotoData.Add(item.Id, (item, CurrentRoomId));
            }

            CurrentRoomPhotosSucceeded = 0;
            CurrentRoomTotalPhotosFound = RoomItemsQueue.Count;
            CurrentRoomUserShouldBeNotified = true;
            ProgressRoomItemsQueue();
        }

        private async void ProgressRoomItemsQueue()
        {
            if (!_waitingForPostItData && RoomItemsQueue.Count > 0)
            {
                _waitingForPostItData = true;
                await Connection.SendToServerAsync(Out.PostItRequestData, RoomItemsQueue.First());
            }
            else if (RoomItemsQueue.Count == 0)
            {
                _ui.UpdateRoomItemsQueueStatusMessage();
                LoadingPhotos = PhotoLoadType.None;

                if (CurrentRoomUserShouldBeNotified && CurrentRoomTotalPhotosFound > 0)
                {
                    CurrentRoomUserShouldBeNotified = false;

                    HabboAlert alert =
                        AlertBuilder.CreateAlert(HabboAlertType.Bubble, CurrentRoomPhotosSucceeded + Constants.OUT_OF + CurrentRoomTotalPhotosFound + Constants.SUCCEEDED_COUNT_DIALOG_BODY)
                            .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }
            }
        }

        public async void SendPhoto(OldPhoto photo)
        {
            byte[] compressed = Flazzy.Compression.ZLIB.Compress(Encoding.UTF8.GetBytes(PhotoConverter.OldToNew(photo).ToString()));
            await Connection.SendToServerAsync(Out.CameraRoomPicture, compressed.Length, compressed);
        }

        private void BuyPhotoBtn_Click(object sender, EventArgs e)
        {
            _waitingForPreview = true;
            BuyPhotoBtn.Enabled = false;
            BuyPhotoBtn.BackgroundImage = Resources.DisabledBuyButton;
            PublishToWebBtn.Enabled = false;
            PublishToWebBtn.BackgroundImage = Resources.DisabledPublishButton;
            IndexDisplayLbl.Text = Constants.PURCHASE_LOADING;
            BuyType = 1;
            SendPhoto(CurrentPhoto);
        }

        private void PreviousPhotoBtn_Click(object sender, EventArgs e)
        {
            CurrentIndex--;
            _ui.Update();
        }
        private void NextPhotoBtn_Click(object sender, EventArgs e)
        {
            CurrentIndex++;
            _ui.Update();
        }

        private void PublishToWebBtn_Click(object sender, EventArgs e)
        {
            _waitingForPreview = true;
            BuyPhotoBtn.Enabled = false;
            BuyPhotoBtn.BackgroundImage = Resources.DisabledBuyButton;
            PublishToWebBtn.Enabled = false;
            PublishToWebBtn.BackgroundImage = Resources.DisabledPublishButton;
            IndexDisplayLbl.Text = Constants.PURCHASE_LOADING;
            BuyType = 2;
            SendPhoto(CurrentPhoto);
        }

        public bool PreFilterMessage(ref Message m)
        {
            return _ui.DragControl(ref m);
        }

        private void HotelExtensionUpBtn_Click(object sender, EventArgs e)
        {
            _ui.RotateCarousel(CarouselDirection.Up);
        }

        private void HotelExtensionDownBtn_Click(object sender, EventArgs e)
        {
            _ui.RotateCarousel(CarouselDirection.Down);
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
                    Settings.Default.Email = LoginEmailTxt.Text;
                    Settings.Default.Password = LoginPasswordTxt.Text;
                    SaveSettings();

                    _ui.HideLogin();
                    await AfterLogin().ConfigureAwait(false);
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

        private async void InitializeSettings()
        {
            if (Settings.Default.AutoLogin)
            {
                LoginEmailTxt.Text = Settings.Default.Email;
                LoginPasswordTxt.Text = Settings.Default.Password;
                AutoLoginBx.Checked = true;
            }
        }

        private void AutoLoginBx_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.AutoLogin = AutoLoginBx.Checked;
            SaveSettings();
        }

        public void SaveSettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
        }

        public HotelEndPoint HotelServer { get; set; }

        private Task InjectGameClientAsync(object sender, RequestInterceptedEventArgs e)
        {
            if (!e.Uri.Query.StartsWith("?" + _randomQuery)) return null;

            Eavesdropper.RequestInterceptedAsync -= InjectGameClientAsync;

            Uri remoteUrl = e.Request.RequestUri;
            string clientPath = Path.Combine(AppDataPath, $@"{Constants.MODDED_CLIENTS_FOLDER_NAME}\{remoteUrl.Host}\{remoteUrl.LocalPath}");

            if (!File.Exists(clientPath))
            {
                _ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT);
                Eavesdropper.ResponseInterceptedAsync += InterceptGameClientAsync;
            }
            else
            {
                _ui.SetStatusMessage(Constants.DISASSEMBLING_CLIENT);
                Game = new HGame(clientPath);
                Game.Disassemble();

                if (Game.IsPostShuffle)
                {
                    _ui.SetStatusMessage(Constants.GENERATING_MESSAGE_HASHES);
                    Game.GenerateMessageHashes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.HASHES_FILE_NAME));
                }
                In = Game.In;
                Out = Game.Out;

                TerminateProxy();
                InterceptConnectionAsync().ConfigureAwait(false);
                e.Request = WebRequest.Create(new Uri(clientPath));
            }

            return null;
        }

        private async Task InterceptGameClientAsync(object sender, ResponseInterceptedEventArgs e)
        {
            if (!e.Uri.Query.StartsWith("?" + _randomQuery)) return;
            if (e.ContentType != Constants.CONTENT_TYPE_FLASH) return;
            Eavesdropper.ResponseInterceptedAsync -= InterceptGameClientAsync;

            string clientPath = Path.Combine(AppDataPath, $@"{Constants.MODDED_CLIENTS_FOLDER_NAME}\{e.Uri.Host}\{e.Uri.LocalPath}");
            string clientDirectory = Path.GetDirectoryName(clientPath);
            Directory.CreateDirectory(clientDirectory);

            _ui.SetStatusMessage(Constants.DISASSEMBLING_CLIENT);
            Game = new HGame(await e.Content.ReadAsStreamAsync().ConfigureAwait(false)) { Location = clientPath };
            Game.Disassemble();

            if (Game.IsPostShuffle)
            {
                _ui.SetStatusMessage(Constants.GENERATING_MESSAGE_HASHES);
                Game.GenerateMessageHashes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.HASHES_FILE_NAME));

                _ui.SetStatusMessage(Constants.MODIFYING_CLIENT);
                Game.DisableHostChecks();
                Game.InjectKeyShouter(4001);
                Game.InjectEndPointShouter(4000);
            }

            In = Game.In;
            Out = Game.Out;
            Game.InjectEndPoint(Constants.LOCALHOST_ENDPOINT, Connection.ListenPort);

            CompressionKind compression = CompressionKind.ZLIB;
#if DEBUG
            compression = CompressionKind.None;
#endif

            _ui.SetStatusMessage(Constants.ASSEMBLING_CLIENT);
            byte[] payload = Game.ToArray(compression);
            e.Headers[HttpResponseHeader.ContentLength] = payload.Length.ToString();

            e.Content = new ByteArrayContent(payload);
            using (FileStream clientStream = File.Open(clientPath, FileMode.Create, FileAccess.Write))
            {
                clientStream.Write(payload, 0, payload.Length);
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
            GameData.Source = body;

            body = body.Replace(".swf", $".swf?{(_randomQuery = Guid.NewGuid())}");
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

        public bool IsIncomingEncrypted { get; private set; }
        public bool HasBeenClosed { get; private set; }

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
                    sharedKeyHex = ("0" + sharedKeyHex);
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
            else if (e.Packet.Id == Out.RequestUserInfo)
            {
                _datagramListener = new DatagramListener(Constants.DATAGRAM_LISTEN_PORT);
                _datagramListener.BuyRequestReceived += ExternalBuyRequestReceived;
                _datagramListener.Listen();

                if (Settings.Default.FirstTimeConnecting)
                {
                    Settings.Default.FirstTimeConnecting = false;
                    SaveSettings();

                    TourRunning = true;
                    Tour = new NewUserTour(Connection, _ui, In.NotificationDialog, In.InClientLink);
                    await Tour.StartAsync();
                }
                else
                {
                    HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.APP_CONNECT_SUCCESS)
                        .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }

                if (GameData.Hotel == HHotel.Com || GameData.Hotel == HHotel.ComTr)
                {
                    if (Settings.Default.VisitedHotels.Contains(GameData.Hotel.ToDomain())) return;

                    Settings.Default.VisitedHotels.Add(GameData.Hotel.ToDomain());
                    SaveSettings();

                    HabboAlert countryIssueAlert = AlertBuilder.CreateAlert(HabboAlertType.PopUp,
                            GameData.Hotel == HHotel.ComTr
                                ? Constants.PHOTO_ISSUE_DIALOG_BODY_TR
                                : Constants.PHOTO_ISSUE_DIALOG_BODY_US)
                        .Title(Constants.PHOTO_ISSUE_DIALOG_TITLE)
                        .EventTitle(Constants.PHOTO_ISSUE_DIALOG_EVENT_TITLE)
                        .EventUrl(string.Empty)
                        .ImageUrl(Constants.PHOTO_ISSUE_DIALOG_IMAGE_URL);

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
                        await Tour.ShowNextHelpBubbleAsync(); break;
                    case Constants.RESPONSE_NAME_EXTERNAL_BUY:
                    {
                        OldPhoto photo = UnconfirmedExternalBuyRequests.Find(p => IdentifierFromUrl(p.Url) == response.Data);
                        if (photo != null)
                        {
                            BuyType = 1;
                            _waitingForPreview = true;
                            SendPhoto(photo);
                        }
                        break;
                    }
                    case Constants.RESPONSE_NAME_CLOSE_FORM:
                        Connection.Disconnect(); break;
                }
            }
            else if (e.Packet.Id == Out.Username && string.IsNullOrWhiteSpace(UsernameOfSession))
            {
                UsernameOfSession = e.Packet.ReadUTF8();
            }
        }

        public string IdentifierFromUrl(string url)
            => url.Remove(0, 24).Split('.')[0];

        private async void ExternalBuyRequestReceived(object sender, OldPhoto e)
        {
            if (HasBeenClosed)
                return;

            UnconfirmedExternalBuyRequests.Add(e);
            string padding = new string(' ', Constants.EXTERNAL_BUY_DIALOG_SPACE_COUNT);
            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, Constants.EXTERNAL_BUY_DIALOG_BODY)
                .EventTitle($"{padding}{Constants.EXTERNAL_BUY_DIALOG_EVENT_TITLE}{padding}")
                .Title(Constants.EXTERNAL_BUY_DIALOG_TITLE)
                .ImageUrl("https:" + e.Url)
                .EventUrl(new EventResponse(Constants.RESPONSE_NAME_EXTERNAL_BUY, IdentifierFromUrl(e.Url)).ToEventString());

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
            else if (e.Packet.Id == In.CameraURL)
            {
                OnPreviewLoaded(e);
            }
            else if (e.Packet.Id == In.PostItData)
            {
                OnPostItDataReceived(e);
            }
            else if (e.Packet.Id == In.RoomWallItems)
            {
                if (LoadingPhotos == PhotoLoadType.None)
                {
                    RoomWallItemsLoaded(e);
                }
                else if (LoadingPhotos == PhotoLoadType.Inventory)
                {
                    HabboAlert alert =
                        AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_INV_BODY)
                            .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                }
                else
                {
                    foreach (int id in RoomItemsQueue)
                    {
                        if (Photos.ContainsKey(id))
                            Photos.Remove(id);
                        if (ExtraPhotoData.ContainsKey(id))
                            ExtraPhotoData.Remove(id);
                    }
                    RoomItemsQueue.Clear();
                    _ui.Update();
                    _ui.UpdateRoomItemsQueueStatusMessage();
                    _waitingForPostItData = false;

                    HabboAlert alert =
                        AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_NEWROOM_BODY)
                            .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                    await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));

                    RoomWallItemsLoaded(e);
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
                    .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
                await Connection.SendToServerAsync(Out.TestInventory);
            }
            else
            {
                HabboAlert alert =
                    AlertBuilder.CreateAlert(HabboAlertType.Bubble, Constants.STILL_BUSY_DIALOG_ROOM_BODY)
                        .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH);

                await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
            }
        }

        private void LoginTitleLbl_Click(object sender, EventArgs e)
        {
            Process.Start(Constants.BASE_URL + Constants.REGISTER_URL);
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
                        .ImageUrl(Constants.BASE_URL + Constants.BUBBLE_ICON_DEFAULT_PATH)
                        .EventUrl(new EventResponse(Constants.RESPONSE_NAME_CLOSE_FORM, string.Empty).ToEventString());

                await Connection.SendToClientAsync(alert.ToPacket(In.NotificationDialog));
            }
        }

        private void ExportCertificateBtn_Click(object sender, EventArgs e)
        {
            var result = CertLocationDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                string path = CertLocationDlg.SelectedPath;
                try
                {
                    Eavesdropper.Certifier.ExportTrustedRootCertificate(Path.Combine(path, Constants.CERT_CERTIFICATE_NAME + Constants.CERT_FILE_EXTENSION));
                    Process.Start(path);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(Constants.EXP_CERT_UNAUTH_BODY, Constants.EXP_CERT_UNAUTH_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

        }
    }
}