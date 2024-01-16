using HabboGallery.Desktop.Web;
using HabboGallery.Desktop.Utilities;


using WindowsFormsLifetime;
using System.Runtime.InteropServices;

namespace HabboGallery.Desktop;

public class Program
{
    public ApiClient Api { get; }

    public HGResources Resources { get; }
    
    public DirectoryInfo DataDirectory { get; }
    public DirectoryInfo ProgramDirectory { get; }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new MainFrm());
    }

    public Program()
    {
        DataDirectory = CreateDataDirectory();
        ProgramDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        Resources = new HGResources();

        Api = new ApiClient(new Uri(Constants.BASE_URL));
    }

    private static DirectoryInfo CreateDataDirectory()
    {
        string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Directory.CreateDirectory(Path.Combine(appdataPath, "HabboGallery"));
    }
}
