using System.Threading.Tasks;

using HabboGallery.UI;
using HabboGallery.Properties;
using HabboGallery.Habbo.Network;

using HabboAlerts;
using HabboGallery.Habbo.Events;

namespace HabboGallery.Habbo.Guidance
{
    public class NewUserTour
    {
        private readonly HConnection _connection;
        private readonly UIUpdater _ui;

        private readonly ushort _alertId;
        private readonly ushort _executeEventId;
        private int _guideHelpBubbleStep;
        private const string IntroDialogTitle = "HabboGallery - New User Guide";
        private const string IntroDialogBody = "<b><font size=\"15\">Welcome to HabboGallery!</font></b><br/><br/>If you see this message," +
                                               " that means you've <b><font color=\"#006400\">successfully connected</font></b> HabboGallery to" +
                                               " Habbo for the very first time!<br><br><br><b>What to do?</b><br>If you would like to get a detailed" +
                                               " guide about how HabboGallery works, <u>click the button below!</u> <b><font color=\"#8b0000\">&nbsp;If" +
                                               " you would rather skip the guide, click the red cross.</font></b><br><br><br><b>Need more help?</b><br><br>" +
                                               "After you complete the guide, you will be given the option to go to our Twitter for additional support.";
        private const string IntroDialogEventTitle = "Start Guide";

        private const string StartingDialogTitle = "HabboGallery - Guide Starting";
        private const string StartingDialogBody = "<b>Sorry for the inconvenience..</b><br/><br/>In order to start the guide, we had to escort you to a random" +
                                                  " room! But we're good to go now.<br/><br/><b>Note:&nbsp;</b>The guide will explain certain buttons in the" +
                                                  " HabboGallery app, so look for flashing buttons!<br/><br/><u>Click the button below to start!</u>";
        private const string StartingDialogEventTitle = "Continue";

        private const string HelpbubbleInventoryBody = "Loading the inventory will make HabboGallery scan for photos.\n\nYou can also click the \"Search\" icon" +
                                                       " in HabboGallery to force your inventory to reload!";
        private const string HelpbubbleBuyPhotoBody = "By clicking the green \"Buy\" button in HabboGallery, you can convert any old photo into a new one!\n\nThis will" +
                                                      " cost 2 Habbo credits.";
        private const string HelpbubblePublishPhotoBody = "By clicking the \"Photo\" icon in HabboGallery, you can publish your old photo to your Habbo profile!" +
                                                          "\n\nThis will cost 10 duckets.";

        private const string OutroDialogTitle = "HabboGallery - Guide Complete!";
        private const string OutroDialogBody = "<b>You've done it!</b><br/><br/>Congratulations! You've completed the guide. You're now allowed to call yourself" +
                                               " a <b>&nbsp;HabboGallery pro!</b><br/><br/><br/><b>Need more help?</b><br/><br/>If you're not feeling like that" +
                                               " much of a HabboGallery pro just yet and need a little more help, feel free to click the link below to be taken" +
                                               " to our Twitter.<br/><br/><font color=\"#FF0000\"><b>Don't need help? Click the red cross.</i></font>";
        private const string OutroDialogEventTitle = "Open Twitter";
        private const string OutroDialogEventUrl = "https://twitter.com/HabboGallery";

        public NewUserTour(HConnection connection, UIUpdater ui, ushort alertId, ushort executeEventId)
        {
            _connection = connection;
            _ui = ui;
            _alertId = alertId;
            _executeEventId = executeEventId;
        }

        public async Task StartAsync()
        {
            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, IntroDialogBody)
                .Title(IntroDialogTitle)
                .EventTitle(IntroDialogEventTitle)
                .EventUrl(HabboEvents.FindFriends);

            await _connection.SendToClientAsync(alert.ToPacket(_alertId));
        }

        public async Task ShowGuideStartedMessageAsync()
        {
            HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, StartingDialogBody)
                .EventTitle(StartingDialogEventTitle)
                .EventUrl(new EventResponse(Constants.RESPONSE_NAME_NUT, string.Empty).ToEventString())
                .Title(StartingDialogTitle);

            await _connection.SendToClientAsync(alert.ToPacket(_alertId));
        }

        public async Task ShowNextHelpBubbleAsync()
        {
            switch (_guideHelpBubbleStep)
            {
                case 0:
                {
                    await _connection.SendToClientAsync(_executeEventId, HabboEvents.ShowHelpBubble(HabboUIControl.BOTTOM_BAR_INVENTORY, HelpbubbleInventoryBody));
                    _ui.FlashButton(ButtonFlash.InventorySearch);
                    break;
                }
                case 1:
                {
                    await _connection.SendToClientAsync(_executeEventId, HabboEvents.ShowHelpBubble(HabboUIControl.CREDITS_BUTTON, HelpbubbleBuyPhotoBody));
                    _ui.FlashButton(ButtonFlash.Purchase);
                    break;
                }
                case 2:
                {
                    await _connection.SendToClientAsync(_executeEventId, HabboEvents.ShowHelpBubble(HabboUIControl.DUCKETS_BUTTON, HelpbubblePublishPhotoBody));
                    _ui.FlashButton(ButtonFlash.PublishToWeb);
                    break;
                }
                case 3:
                {
                    HabboAlert alert = AlertBuilder.CreateAlert(HabboAlertType.PopUp, OutroDialogBody)
                        .EventTitle(OutroDialogEventTitle)
                        .Title(OutroDialogTitle)
                        .EventUrl(OutroDialogEventUrl, true);

                    _ui.FlashButton(ButtonFlash.None);
                    await _connection.SendToClientAsync(alert.ToPacket(_alertId));

                    _ui.Target.TourRunning = false;
                    break;
                }
            }

            _guideHelpBubbleStep++;
        }
    }
}
