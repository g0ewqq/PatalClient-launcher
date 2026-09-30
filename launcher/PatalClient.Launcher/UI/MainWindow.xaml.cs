using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        LogoMarkHost.Content = Resources["LogoMark"];
        LogoHost.Content = UI.PatalLogo.CreateHero(76);
        VersionSelector.ItemsSource = VersionRegistry.Versions;
        VersionSelector.SelectedItem = VersionRegistry.Find(config.SelectedVersionId) ?? VersionRegistry.Default;

        RefreshPrimaryButton();
        UpdateStatus(null);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (UI.Motion.ReducedMotion)
            return;

        HeroHost.Opacity = 0;
        ControlsHost.Opacity = 0;
        FooterHost.Opacity = 0;

        UI.Motion.FadeScale((FrameworkElement)LogoHost.Content, 0.94, UI.Motion.Entrance);
        RunEntrance(HeroHost, TimeSpan.FromMilliseconds(90), 6);
        RunEntrance(ControlsHost, TimeSpan.FromMilliseconds(170), 5);
        RunEntrance(FooterHost, TimeSpan.FromMilliseconds(240), 0);
        BeginAmbient();
    }

    private void RunEntrance(FrameworkElement element, TimeSpan delay, double slideFrom)
    {
        var storyboard = new Storyboard { BeginTime = delay };

        var fade = new DoubleAnimation(0, 1, UI.Motion.Entrance) { EasingFunction = UI.Motion.EaseOut };
        Storyboard.SetTarget(fade, element);
        Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
        storyboard.Children.Add(fade);

        if (slideFrom != 0)
        {
            var translate = new TranslateTransform(slideFrom, 0);
            element.RenderTransform = translate;
            var slide = new DoubleAnimation(slideFrom, 0, UI.Motion.Entrance) { EasingFunction = UI.Motion.EaseOut };
            Storyboard.SetTarget(slide, element);
            Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.Y"));
            storyboard.Children.Add(slide);
        }

        storyboard.Begin(this);
    }

    private void BeginAmbient()
    {
        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true };
        var drift = new DoubleAnimation(0.55, 1, TimeSpan.FromSeconds(16))
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(drift, AmbientGlow);
        Storyboard.SetTargetProperty(drift, new PropertyPath("(UIElement.Opacity)"));
        storyboard.Children.Add(drift);
        storyboard.Begin(AmbientGlow, true);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (UI.Motion.ReducedMotion || AmbientGlow == null)
            return;

        if (WindowState == WindowState.Minimized)
            AmbientGlow.BeginAnimation(OpacityProperty, null);
        else
            BeginAmbient();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 || e.ButtonState != System.Windows.Input.MouseButtonState.Pressed) return;
        DragMove();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new SettingsWindow(_config) { Owner = this };
        settings.ShowDialog();
    }

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

        FooterStatusText.Text = _state switch
        {
            LauncherState.Ready => "READY",
            LauncherState.Injecting => "PREPARING",
            LauncherState.Active => "ACTIVE",
            LauncherState.Ejecting => "EJECTING",
            _ => "OFFLINE"
        };

        FooterStatusText.SetResourceReference(TextBlock.ForegroundProperty,
            isError ? "ErrorBrush" : "TextSecondaryBrush");

        StatusDot.Fill = (Brush)FindResource(isError ? "ErrorBrush" : _state switch
        {
            LauncherState.Active => "SuccessBrush",
            LauncherState.Ready => "AccentBrush",
            _ => "TextDimBrush"
        });

        if (!UI.Motion.ReducedMotion)
        {
            var fade = new DoubleAnimation(0.35, 1, UI.Motion.Fast);
            StatusDot.BeginAnimation(OpacityProperty, fade);
        }

        FooterVersionText.Text = SelectedVersion.VersionId;
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
