namespace HabboGallery
{
    public static class Constants
    {
        public const string APP_NAME = "HabboGallery";
        public const double APP_VERSION = 0.25;

        public const string BASE_URL = "https://habbo.gallery/";

        public const ushort PROXY_PORT = 8282;

        public const string INTERCEPTING_CLIENT = "Intercepting Client..";
        public const string INTERCEPTING_CONNECTION = "Intercepting Connection..";
        public const string INTERCEPTING_CLIENT_PAGE = "Intercepting Client Page..";

        public const string MODIFYING_CLIENT = "Modifying Client..";
        public const string INJECTING_CLIENT = "Injecting Client..";
        public const string GENERATING_MESSAGE_HASHES = "Generating Message Hashes..";

        public const string ASSEMBLING_CLIENT = "Assembling Client..";
        public const string DISASSEMBLING_CLIENT = "Disassembling Client..";

        public const string CONNECTED = "Connected!";
        public const string DISCONNECTED = "Disconnected!";

        public const string LOGIN_FAILED = "Login failed!";
        public const string LOGIN_ERROR = "Unknown error!";

        public const string PURCHASE_LOADING = "Loading";

        public const string EXTERNAL_BUY_DIALOG_BODY = "<font size=\"15\">&nbsp;&nbsp;<b>Buy Photo</b></font><br><br>&nbsp;&nbsp;A request to buy this photo " +
                                                       "has been received.<br>&nbsp;&nbsp;If this was not you, please close this message.<br><br>&nbsp;&nbsp;" +
                                                       "Buying a photo costs 2 Habbo Credits.";
        public const string EXTERNAL_BUY_DIALOG_TITLE = "HabboGallery - External Request";
        public const string EXTERNAL_BUY_DIALOG_EVENT_TITLE = "Buy";
        public const int EXTERNAL_BUY_DIALOG_SPACE_COUNT = 41;

        public const string SCANNING_INVENTORY = "Looking for photos in your inventory..";
        public const string SCANNING_INVENTORY_DONE = "found in your inventory!";

        public const string SCANNING_EMPTY = "No";

        public const string APP_CONNECT_SUCCESS = "HabboGallery has successfully connected to Habbo, woo!";

        public const string SCANNING_WALLITEMS_DONE = "found in this room! ";
        public const string SCANNING_WALLITEMS_UNDISC = " of them have not been discovered before!";

        public const string SCANNING_MULTI = " photos were ";
        public const string SCANNING_SINGLE = " photo was ";

        public const string CERT_CERTIFICATE_NAME = "HabboGallery Root Certificate";
        public const string CERT_FILE_EXTENSION = ".cer";

        public const string UPDATE_FOUND_BODY = "A newer version of HabboGallery Desktop was found, would you like to download it?";
        public const string UPDATE_FOUND_TITLE = "HabboGallery ~ Update Found";

        public const string BUBBLE_ICON_DEFAULT_PATH = "desktop/icon.png";

        public const string MODDED_CLIENTS_FOLDER_NAME = "Modified Clients";

        public const string HASHES_FILE_NAME = "Hashes.ini";

        public const string LOCALHOST_ENDPOINT = "127.0.0.1";

        public const int DATAGRAM_LISTEN_PORT = 9119;

        public const string RESPONSE_NAME_NUT = "newUserTour";
        public const string RESPONSE_NAME_EXTERNAL_BUY = "confirmExternalBuy";
        public const string RESPONSE_NAME_CLOSE_FORM = "closeForm";

        public const string CONTENT_TYPE_FLASH = "application/x-shockwave-flash";
        public const string CONTENT_TYPE_TEXT = "text";

        public const string SANTORINI_FURNI_NAME = "santorini_c17_bar_64_a_0_0";
        public const string ADS_BACKGROUND_FURNI_NAME = "ads_mpu_160_64_a_4_0";

        public const string PHOTO_ISSUE_DIALOG_TITLE = "Uh oh...";

        public const string PHOTO_ISSUE_DIALOG_BODY_TR = "<font size=\"15\"><b>Issue regarding habbo.com.tr</b></font><br/><br/>Unfortunately, " +
                                                         "Habbo Turkey was released after the camera feature was removed from Habbo. This means that there" +
                                                         " are no photos to discover in this hotel, sorry!";
        public const string PHOTO_ISSUE_DIALOG_BODY_US = "<font size=\"15\"><b>Issue regarding habbo.com</b></font><br/><br/>Unfortunately, during " +
                                                         "the merge of all English speaking hotels into habbo.com, a lot of the old photos have been " +
                                                         "<b>permanently deleted</b>.<br/><br/>As a result of that, we are only able to show you photos " +
                                                         "that were taken on <b>&nbsp;Habbo USA</b>. If your account originates from another hotel (e.g. UK, " +
                                                         "CA or AU), you will not be able to retrieve your photos, sorry!";
        public const string PHOTO_ISSUE_DIALOG_EVENT_TITLE = "I understand :(";
        public const string PHOTO_ISSUE_DIALOG_IMAGE_URL = "https://images.habbo.com/c_images/Security/safetytips6_n.png";

        public const string STILL_BUSY_DIALOG_INV_BODY = "Photos in this room have not been loaded. Please wait, we're still processing the photos in your inventory!";
        public const string STILL_BUSY_DIALOG_ROOM_BODY = "Inventory has not been loaded. Please wait, we're still processing some photos that were in found in a room.";
        public const string STILL_BUSY_DIALOG_NEWROOM_BODY = "You entered a new room before all photos in the previous room were loaded. The queue for that room has been cleared!";

        public const string REGISTER_URL = "register";

        public const string OUT_OF = " out of ";
        public const string SUCCEEDED_COUNT_DIALOG_BODY = " were successfully recovered. Photos that failed might have been permanently deleted.";

        public const string FORM_CLOSED_DIALOG_BODY = "HabboGallery will continue running in the background to prevent you from being disconnected from Habbo. It will not do anything else.\n\nClick here if you want to close it entirely (Disconnects you from Habbo!)";

        public const string EXP_CERT_UNAUTH_TITLE = "HabboGallery ~ Unauthorized";
        public const string EXP_CERT_UNAUTH_BODY = "HabboGallery does not have access to the chosen directory. Please try another one, or run HabboGallery as administrator in order to save to this directory.";

        public const string UPDATE_URL = "download";
        public const string UP_TO_DATE_MESSAGE = "No updates found!";
    }
}