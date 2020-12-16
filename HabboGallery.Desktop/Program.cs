using System;
using System.IO;
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
        public ApiClient Api { get; }

        public Incoming In { get; set; }
        public Outgoing Out { get; set; }

        public HGameData GameData { get; }
        public HConnection Connection { get; }

        public HGResources Resources { get; }
        public HGConfiguration Configuration { get; }
        
        public DirectoryInfo DataDirectory { get; }
        
        public static Program Master { get; private set; }

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
            if (OperatingSystem.IsWindowsVersionAtLeast(7))
            {
                Eavesdropper.Terminate();
                Eavesdropper.Certifier = new CertificateManager("HabboGallery", "HabboGallery Root Certificate");
            }
            else throw new PlatformNotSupportedException("This operating system is not supported! The minimum requirement is Windows 7 and Windows 10 is highy recommended!");
            
            Resources = new HGResources();
            Configuration = new HGConfiguration();

            Api = new ApiClient(new Uri(Constants.BASE_URL));

            GameData = new HGameData();
            Connection = new HConnection();

            DataDirectory = CreateDataDirectory();
        }

        private static DirectoryInfo CreateDataDirectory()
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Directory.CreateDirectory(Path.Combine(appdataPath, "HabboGallery"));
        }
    }
}
