using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PatalClient.Launcher.Models;
using PatalClient.Launcher.Services;
using PatalClient.Launcher.Versions;

namespace PatalClient.Launcher;

public partial class MainWindow : Window
{
    private readonly LauncherConfiguration _config;
    private readonly IClientIntegration _integration;
    private LauncherState _state = LauncherState.Disconnected;
    private MinecraftVersion SelectedVersion => (MinecraftVersion)VersionSelector.SelectedItem;

    public MainWindow(LauncherConfiguration config)
    {
        InitializeComponent();
        _config = config;
        _integration = new NativeClientIntegration();

        Resources["LogoMark"] = UI.PatalLogo.CreateMark(15);
        LogoHost.Content = UI.PatalLogo.CreateHero(62);
        VersionSelector.ItemsSource = VersionRegistry.Versions;
        VersionSelector.SelectedItem = VersionRegistry.Find(config.SelectedVersionId) ?? VersionRegistry.Default;

        RefreshPrimaryButton();
        UpdateStatus(null);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 || e.ButtonState != MouseButtonState.Pressed) return;
        if (e.OriginalSource is DependencyObject source &&
            source is System.Windows.Controls.TextBlock or System.Windows.Shapes.Shape)
        {
            DragMove();
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new SettingsWindow(_config) { Owner = this };
        settings.ShowDialog();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void VersionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;

        var version = SelectedVersion;
        _config.SelectedVersionId = version.VersionId;
        _config.Save();
        Logger.Info($"Selected version set to {version.VersionId}");

        SetState(LauncherState.Ready);
        RefreshPrimaryButton();
        UpdateStatus(null);
    }

    private async void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_state is LauncherState.Active)
        {
            SetState(LauncherState.Ejecting);
            RefreshPrimaryButton();
            UpdateStatus(null);
            var eject = await Task.Run(_integration.Eject);
            ApplyResult(eject, LauncherState.Ready);
            return;
        }

        var version = SelectedVersion;

        SetState(LauncherState.Injecting);
        RefreshPrimaryButton();
        UpdateStatus(null);

        var detect = await Task.Run(_integration.DetectMinecraft);
        if (!detect.Succeeded) { ApplyResult(detect, LauncherState.Error); return; }

        var validate = await Task.Run(() => _integration.ValidateVersion(version));
        if (!validate.Succeeded) { ApplyResult(validate, LauncherState.Error); return; }

        var init = await Task.Run(() => _integration.Initialize(version));
        if (!init.Succeeded) { ApplyResult(init, LauncherState.Error); return; }

        var inject = await Task.Run(() => _integration.Inject(version));
        ApplyResult(inject, LauncherState.Ready);
    }

    private void ApplyResult(IntegrationResult result, LauncherState fallbackState)
    {
        SetState(result.Succeeded ? LauncherState.Active : fallbackState);
        RefreshPrimaryButton();
        UpdateStatus(result.Succeeded ? null : DescribeFailure(result));
    }

    private void SetState(LauncherState state)
    {
        _state = state == LauncherState.Error ? LauncherState.Disconnected : state;
    }

    private void RefreshPrimaryButton()
    {
        PrimaryButton.IsEnabled = _state is LauncherState.Disconnected or LauncherState.Ready;
        PrimaryButton.Content = _state switch
        {
            LauncherState.Injecting => "PREPARING",
            LauncherState.Active => "EJECT",
            LauncherState.Ejecting => "EJECTING",
            _ => "INJECT"
        };
    }

    private void UpdateStatus(string? message)
    {
        var isError = message is not null;

        StatusText.Text = message ?? _state switch
        {
            LauncherState.Disconnected => "Native integration not yet available — injection arrives in a later release.",
            LauncherState.Ready => "Ready. The client component was found and can be injected.",
            LauncherState.Injecting => "Preparing the client component…",
            LauncherState.Active => "PatalClient is running inside Minecraft.",
            LauncherState.Ejecting => "Stopping the client…",
            _ => string.Empty
        };

        StatusText.SetResourceReference(TextBlock.ForegroundProperty,
            isError ? "ErrorBrush" : "TextDimBrush");

        FooterVersionText.Text = SelectedVersion.VersionId;
        FooterStatusText.Text = _state switch
        {
            LauncherState.Ready => "R E A D Y",
            LauncherState.Injecting => "I N J E C T I N G",
            LauncherState.Active => "A C T I V E",
            LauncherState.Ejecting => "E J E C T I N G",
            _ => "O F F L I N E"
        };

        FooterStatusText.SetResourceReference(TextBlock.ForegroundProperty,
            isError ? "ErrorBrush" : "TextBrush");

        StatusDot.Fill = (Brush)FindResource(isError ? "ErrorBrush" : _state switch
        {
            LauncherState.Active => "SuccessBrush",
            LauncherState.Ready => "AccentBrush",
            _ => "TextDimBrush"
        });
    }

    private static string DescribeFailure(IntegrationResult result) => result.Failure switch
    {
        IntegrationFailure.MinecraftNotDetected => "Minecraft was not detected. Start Minecraft, then inject again.",
        IntegrationFailure.UnsupportedVersion => result.Detail ?? "This Minecraft version is not supported yet.",
        IntegrationFailure.NativeComponentMissing => result.Detail ?? "The client component for this version is missing.",
        IntegrationFailure.IntegrationUnavailable => result.Detail ?? "The client integration is not available yet.",
        _ => result.Detail ?? "An unexpected error occurred."
    };

    protected override void OnClosing(CancelEventArgs e)
    {
        _config.Save();
        base.OnClosing(e);
    }
}
