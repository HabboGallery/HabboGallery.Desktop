using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using HabboGallery.Desktop.Web;
using HabboGallery.Desktop.Utilities;
using HabboGallery.Desktop.Habbo.Network;

using Sulakore.Habbo.Web;
using Sulakore.Habbo.Messages;

using Eavesdrop;

namespace HabboGallery.Desktop
{
    public class Program
    {
        public ApiClient ApiClient { get; }

        public HGResources Resources { get; }
        public HGConfiguration Configuration { get; }

        public DirectoryInfo DataDirectory { get; }

        public Incoming In { get; }
        public Outgoing Out { get; }

        public HGame Game { get; set; } //TODO: Kill HGame
        
        public HGameData GameData { get; }
        public HConnection Connection { get; }

        public HGConfiguration Config { get; }
        public bool IsConnected => Connection.IsConnected;


        public static Program Master { get; private set; }

        public static Font DefaultFont { get; } = new Font("Microsoft Sans Serif", 8f);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Master = new Program();
            Application.Run(new MainFrm());
        }

        public Program()
        {
            Eavesdropper.Certifier = new CertificateManager("HabboGallery", "HabboGallery Root Certificate");

                Eavesdropper.Terminate();
            //Load and cache all embedded resources
            Resources = new HGResources();

            Configuration = new HGConfiguration();

            GameData = new HGameData();
            Connection = new HConnection();
            //Connection.DataOutgoing += HandleOutgoing;
            //Connection.DataIncoming += HandleIncoming;
            //
            //Connection.Connected += ConnectionOpened;
            //Connection.Disconnected += ConnectionClosed;

            In = new Incoming();
            Out = new Outgoing();

            if (Eavesdropper.Certifier.CreateTrustedRootCertificate())
            {
                //Eavesdropper.ResponseInterceptedAsync += InterceptClientPageAsync;

                //Eavesdropper.Initiate(Constants.PROXY_PORT);
                //_ui.SetStatusMessage(Constants.INTERCEPTING_CLIENT_PAGE);
            }

            DataDirectory = CreateAppDataDirectory();
        }

        private DirectoryInfo CreateAppDataDirectory()
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Directory.CreateDirectory(Path.Combine(appdataPath, "HabboGallery"));
        }
    }
}
