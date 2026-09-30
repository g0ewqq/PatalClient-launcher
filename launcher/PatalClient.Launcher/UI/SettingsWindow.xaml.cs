using System.Windows;
using System.Windows.Controls;
using PatalClient.Launcher.Models;

namespace PatalClient.Launcher;

public partial class SettingsWindow : Window
{
    private readonly LauncherConfiguration _config;

    public SettingsWindow(LauncherConfiguration config)
    {
        InitializeComponent();
        _config = config;
        UI.Motion.FadeScale(this, 0.985, UI.Motion.Entrance);
        ShowCategory("General");
    }

    private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (CategoryList.SelectedItem is ListBoxItem item)
            ShowCategory(item.Content.ToString()!);
    }

    private void ShowCategory(string category)
    {
        CategoryTitle.Text = category;
        PanelHost.Content = category switch
        {
            "General" => BuildGeneralPanel(),
            "Minecraft" => BuildMinecraftPanel(),
            _ => BuildPlaceholderPanel(category)
        };
    }

    private UIElement BuildGeneralPanel()
    {
        var panel = new StackPanel();

        panel.Children.Add(MakeToggle(
            "Remember window preferences",
            _config.RememberWindowSize,
            value => { _config.RememberWindowSize = value; _config.Save(); }));

        panel.Children.Add(MakeToggle(
            "Reduce motion",
            _config.ReducedMotion,
            value => { _config.ReducedMotion = value; UI.Motion.ReducedMotion = value; _config.Save(); }));

        return panel;
    }

    private UIElement BuildMinecraftPanel()
    {
        var panel = new StackPanel();

        panel.Children.Add(MakeToggle(
            "Check for updates on start",
            _config.CheckForUpdatesOnStart,
            value => { _config.CheckForUpdatesOnStart = value; _config.Save(); }));

        panel.Children.Add(new TextBlock
        {
            Text = $"Minecraft directory detection is handled automatically. Selected version: {_config.SelectedVersionId}.",
            Style = (Style)Application.Current.FindResource("SecondaryTextStyle"),
            Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextDimBrush"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 14, 0, 0)
        });

        return panel;
    }

    private static UIElement BuildPlaceholderPanel(string category)
    {
        return new TextBlock
        {
            Text = $"{category} settings arrive in a later phase.",
            Style = (Style)Application.Current.FindResource("CaptionTextStyle"),
            Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextDimBrush")
        };
    }

    private static StackPanel MakeToggle(string label, bool value, Action<bool> onChanged)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        var checkBox = new CheckBox
        {
            IsChecked = value,
            VerticalAlignment = VerticalAlignment.Center
        };
        checkBox.Checked += (_, _) => onChanged(true);
        checkBox.Unchecked += (_, _) => onChanged(false);

        panel.Children.Add(checkBox);
        panel.Children.Add(new TextBlock
        {
            Text = label,
            Style = (Style)Application.Current.FindResource("SecondaryTextStyle"),
            Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("TextBrush"),
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        var wrapper = new StackPanel();
        wrapper.Children.Add(panel);
        return wrapper;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
