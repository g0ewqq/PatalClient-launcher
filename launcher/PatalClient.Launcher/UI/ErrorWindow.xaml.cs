using System.Windows;

namespace PatalClient.Launcher;

public partial class ErrorWindow : Window
{
    public ErrorWindow(Exception exception)
    {
        InitializeComponent();
        MessageText.Text = "An unexpected error occurred in the launcher. The launcher will keep running.";
        Logger.Error("Error dialog shown", exception);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
