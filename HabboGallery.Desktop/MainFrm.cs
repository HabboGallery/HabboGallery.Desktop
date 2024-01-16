using System.Text;
using System.Diagnostics;

using HabboGallery.Desktop.UI;
using HabboGallery.Desktop.Web;
using HabboGallery.Desktop.Habbo;
using HabboGallery.Desktop.Controls;
using HabboGallery.Desktop.Web.Json;
using HabboGallery.Desktop.Utilities;

#nullable enable
namespace HabboGallery.Desktop;

public partial class MainFrm : NotifiableForm, IMessageFilter
{
    public readonly Version Version = new Version(1, 0, 0);

    private const int GET_ITEMDATA_DELAY = 780;
    private const int PUBLISH_QUEUE_POLL_DELAY = 1000;

    public ApiClient Api => Master.Api;
    
    public HGResources Resources => Master.Resources;

    private readonly UIUpdater _ui;
    
    //TODO: Lazy loading the images in carousel
    public Dictionary<long, Image> ImageCache { get; }
    public Dictionary<long, GalleryRecord> Photos { get; }

    public int CurrentIndex { get; set; }
    public GalleryRecord? CurrentPhoto => Photos?.Values.ToArray()[CurrentIndex]; //TODO:

    private Queue<long> _roomPhotoQueue;
    private Queue<PhotoItem> _photoPublishingQueue;
    private Dictionary<long, HNewWallItem> _roomPhotoItems;
    
    public string? SessionUsername { get; set; }
    public long CurrentRoomId { get; private set; }

    private bool _isProcessingItems;

    public bool HasBeenClosed { get; private set; }

    public MainFrm()
    {
        Application.AddMessageFilter(this);

        ImageCache = new Dictionary<long, Image>();
        Photos = new Dictionary<long, GalleryRecord>();
        
        _roomPhotoQueue = new Queue<long>();
        _photoPublishingQueue = new Queue<PhotoItem>();

        _roomPhotoItems = new Dictionary<long, HNewWallItem>();

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
                Process.Start(new ProcessStartInfo(Constants.BASE_URL + "/download") { UseShellExecute = true });

                Environment.Exit(0);
            }
        }
        else _ui.SetStatusMessage(Constants.UP_TO_DATE_MESSAGE);
    }
            
    private void OnInventoryPush(DataInterceptedEventArgs e)
    {
        e.Continue();

        IEnumerable<HNewItem> items = HNewItem.Parse(e.Packet)
            .Where(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id));

        int itemCount = items.Count();

        string message = (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
            + (itemCount == 1 ? Constants.SCANNING_SINGLE : Constants.SCANNING_MULTI)
            + Constants.SCANNING_INVENTORY_DONE;
        
        //TODO: message
        //TODO: Show user the queueu in photo publishing pipeline

        _isProcessingItems = true;

        //Send all photo items in inventory to photo data processing pipeline.
        foreach (HNewItem item in items)
        {
            string extraData = ((HLegacyStuffData)item.StuffData).Data;

            var photoItem = PhotoItem.Create(item.Id, extraData, Hotel, SessionUsername,
                roomId: null);

            _photoPublishingQueue.Enqueue(photoItem);
        }
    }
    private async void OnItems(DataInterceptedEventArgs e)
    {
        e.Continue();

        IEnumerable<HNewWallItem> items = HNewWallItem.Parse(e.Packet)
            .Where(i => i.TypeId == 3 && !Photos.ContainsKey(i.Id) &&
            !_roomPhotoQueue.Contains(i.Id));

        IEnumerable<long>? unknownIds = await Api.BatchCheckExistingIdsAsync(items.Select(i => i.Id), Hotel).ConfigureAwait(false);

        int itemCount = items.Count();
        int unknownCount = unknownIds?.Count() ?? 0;

        string alertMessage = (itemCount == 0 ? Constants.SCANNING_EMPTY : itemCount.ToString())
            + (itemCount == 0 || itemCount > 1 ? Constants.SCANNING_MULTI : Constants.SCANNING_SINGLE)
            + Constants.SCANNING_WALLITEMS_DONE;

        if (unknownCount > 0)
            alertMessage += " " + unknownCount + Constants.SCANNING_WALLITEMS_UNDISC;

        //TODO: alertMessage

        _isProcessingItems = true;

        foreach (HNewWallItem item in items)
        {
            if (unknownIds?.Contains(item.Id) ?? true)
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
    private void OnItemData(DataInterceptedEventArgs e)
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
                _roomPhotoItems.TryGetValue(id, out HNewWallItem? roomItem);

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
    private ValueTask<int> ProcessPhotoQueueAsync()
    {
        if (_roomPhotoQueue.Count <= 1)
            _isProcessingItems = false;

        return Connection.SendToServerAsync(Out.GetItemData, _roomPhotoQueue.Peek());
    }

    private async Task GetKnownPhotoByIdAsync(long photoId)
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
                _ui.HideLogin();
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

        if (e.Packet.Id == 4000)
        {
            string nonce = string.Empty;
            string hex = e.Packet.ReadUTF8(0);
            for (var i = 0; i < 8; i++)
            {
                nonce += hex.Substring(i * 3, 2);
            }
            _nonce = Convert.FromHexString(nonce);
        }
        else if (e.Packet.Id == 208)
        {
            e.WaitUntil = _initializeStreamCiphersTask = InitializeStreamCiphersAsync();
        }
        else if (e.Packet.Id == 5)
        {
            long sessionId = e.Packet.ReadInt64();
            SessionUsername = e.Packet.ReadUTF8();

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
    }

    public void HandleIncoming(object? sender, DataInterceptedEventArgs e)
    {
        if (HasBeenClosed)
        {
            e.Continue();
            return;
        }

        if (e.Packet.Id == 279) e.WaitUntil = _initializeStreamCiphersTask;
        else if (e.Packet.Id == 45) OnItems(e);
        else if (e.Packet.Id == 48) OnItemData(e);
        else if (e.Packet.Id == 69) OnRoomReady(e);
        else if (e.Packet.Id == 140) OnInventoryPush(e);
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

        Connection.SendToServerAsync(404);
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
}