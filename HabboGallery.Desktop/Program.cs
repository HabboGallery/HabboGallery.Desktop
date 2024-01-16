﻿using HabboGallery.Desktop.Web;
using HabboGallery.Desktop.Utilities;
using HabboGallery.Desktop.Habbo.Network;

using Sulakore.Habbo.Web;

using Eavesdrop;

using WindowsFormsLifetime;
using System.Runtime.InteropServices;

namespace HabboGallery.Desktop;

public class Program
{
    private readonly string _configPath;
    
    public ApiClient Api { get; }

    public HConnection Connection { get; }

    public HGResources Resources { get; }
    public HGConfiguration Configuration { get; }
    
    public DirectoryInfo DataDirectory { get; }
    public DirectoryInfo ProgramDirectory { get; }
    
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
        DataDirectory = CreateDataDirectory();
        ProgramDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        _configPath = Path.Combine(DataDirectory.FullName, "config.json");

        Resources = new HGResources();
        Configuration = HGConfiguration.Create(_configPath);

        Api = new ApiClient(new Uri(Constants.BASE_URL));

        Connection = new HConnection();
    }

    private static DirectoryInfo CreateDataDirectory()
    {
        string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Directory.CreateDirectory(Path.Combine(appdataPath, "HabboGallery"));
    }

    public void SaveConfig() => Configuration.Save(_configPath);
}
