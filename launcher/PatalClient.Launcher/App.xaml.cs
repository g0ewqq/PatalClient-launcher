using System.Windows;
using PatalClient.Launcher.Models;

namespace PatalClient.Launcher;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            Logger.Error("Unhandled UI exception", args.Exception);
            args.Handled = true;
            new ErrorWindow(args.Exception).ShowDialog();
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Logger.Error("Fatal unhandled exception", args.ExceptionObject as Exception);

        var config = LauncherConfiguration.Load();
        Logger.Info($"PatalClient launcher started ({LauncherConfiguration.LauncherVersion})");

        var window = new MainWindow(config);
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.Info("PatalClient launcher exited");
        base.OnExit(e);
    }
}
