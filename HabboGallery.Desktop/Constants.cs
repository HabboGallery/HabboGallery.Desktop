namespace HabboGallery.Desktop
{
    //TODO: Localizations
    public static class Constants
    {
        public const string BASE_URL = "https://habbo.gallery";

        public const ushort PROXY_PORT = 8282;
        
        public const string PHOTO_ISSUE_DIALOG_IMAGE_URL = "https://images.habbo.com/c_images/Security/safetytips6_n.png";

        public const string STANDING_BY = "Standing By...";

        public const string INTERCEPTING_CLIENT = "Intercepting Client...";
        public const string INTERCEPTING_CONNECTION = "Intercepting Connection...";
        public const string INTERCEPTING_CLIENT_PAGE = "Intercepting Client Page...";
        public const string INTERCEPTING_CLIENT_REQUEST_RESPONSE = "Intercepting Client Request/Response";

        public const string MODIFYING_CLIENT = "Modifying Client...";
        public const string INJECTING_CLIENT = "Injecting Client...";

        public const string ASSEMBLING_CLIENT = "Assembling Client...";
        public const string DISASSEMBLING_CLIENT = "Disassembling Client...";

        public const string CONNECTED = "Connected!";
        public const string DISCONNECTED = "Disconnected!";

        public const string LOGIN_FAILED = "Login failed!";
        public const string LOGIN_ERROR = "Unknown error!";

        public const string SCANNING_INVENTORY = "Looking for photos in your inventory..";
        public const string SCANNING_INVENTORY_DONE = "found in your inventory!";

        public const string SCANNING_EMPTY = "No";

        public const string APP_CONNECT_SUCCESS = "HabboGallery has successfully connected to Habbo, woo!";

        public const string SCANNING_WALLITEMS_DONE = "found in this room! ";
        public const string SCANNING_WALLITEMS_UNDISC = " of them have not been discovered before!";

        public const string SCANNING_MULTI = " photos were ";
        public const string SCANNING_SINGLE = " photo was ";

        public const string UPDATE_FOUND_BODY = "A newer version of HabboGallery Desktop was found, would you like to download it?";
        public const string UPDATE_FOUND_TITLE = "HabboGallery ~ Update Found";

        public const string INTRO_DIALOG_TITLE = "HabboGallery - Welcome!";
        public const string INTRO_DIALOG_BODY = "<b><font size=\"15\">Welcome to HabboGallery!</font></b><br/><br/>If you see this message," +
                                               " that means you've <b><font color=\"#006400\">successfully connected</font></b> HabboGallery to" +
                                               " Habbo for the very first time!<br><br>";

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

        public const string STILL_BUSY_DIALOG_INV_BODY = "Photos in this room have not been loaded. Please wait, we're still processing the photos in your inventory!";
        public const string STILL_BUSY_DIALOG_NEWROOM_BODY = "You entered a new room before all photos in the previous room were loaded. The queue for that room has been cleared!";

        public const string OUT_OF = " out of ";
        public const string SUCCEEDED_COUNT_DIALOG_BODY = " were successfully recovered. Photos that failed might have been permanently deleted.";

        public const string FORM_CLOSED_DIALOG_BODY = "HabboGallery will continue running in the background to prevent you from being disconnected from Habbo. It will not do anything else.\n\nClick here if you want to close it entirely (Disconnects you from Habbo!)";

        public const string EXP_CERT_UNAUTH_TITLE = "HabboGallery ~ Unauthorized";
        public const string EXP_CERT_UNAUTH_BODY = "HabboGallery does not have access to the chosen directory. Please try another one, or run HabboGallery as administrator in order to save to this directory.";

        public const string UP_TO_DATE_MESSAGE = "No updates found!";
    }
}