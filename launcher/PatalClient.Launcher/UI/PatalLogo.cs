using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PatalClient.Launcher.UI;

public static class PatalLogo
{
    private static readonly Color Accent = Color.FromRgb(0x8A, 0x99, 0xAC);
    private static readonly Color Bar = Color.FromRgb(0xE9, 0xEB, 0xED);

    public static FrameworkElement CreateMark(double size = 14)
    {
        var grid = new Grid { Width = size, Height = size };

        var shield = new Path
        {
            Data = Geometry.Parse($"M {size / 2} 1 L {size - 1} {size * 0.20} L {size - 1} {size * 0.55} C {size - 1} {size * 0.78} {size * 0.78} {size - 1} {size / 2} {size - 1} C {size * 0.22} {size - 1} 1 {size * 0.78} 1 {size * 0.55} L 1 {size * 0.20} Z"),
            Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
            Stroke = new SolidColorBrush(Accent),
            StrokeThickness = Math.Max(1, size * 0.075)
        };
        grid.Children.Add(shield);

        var bar = new Rectangle
        {
            Width = size * 0.10,
            Height = size * 0.34,
            RadiusX = size * 0.05,
            RadiusY = size * 0.05,
            Fill = new SolidColorBrush(Bar),
            Margin = new Thickness(0, 0, 0, size * 0.10),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        grid.Children.Add(bar);

        return grid;
    }

    public static FrameworkElement CreateHero(double size = 62)
    {
        var grid = new Grid { Width = size, Height = size };

        var shield = new Path
        {
            Data = Geometry.Parse($"M {size / 2} {size * 0.02} L {size * 0.97} {size * 0.22} L {size * 0.97} {size * 0.55} C {size * 0.97} {size * 0.80} {size * 0.76} {size * 0.97} {size / 2} {size * 0.97} C {size * 0.24} {size * 0.97} {size * 0.03} {size * 0.80} {size * 0.03} {size * 0.55} L {size * 0.03} {size * 0.22} Z"),
            Fill = new SolidColorBrush(Color.FromArgb(0x0A, 0xE9, 0xEB, 0xED)),
            Stroke = new SolidColorBrush(Accent),
            StrokeThickness = Math.Max(1.5, size * 0.045),
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round
        };
        grid.Children.Add(shield);

        var bar = new Rectangle
        {
            Width = size * 0.085,
            Height = size * 0.34,
            RadiusX = size * 0.0425,
            RadiusY = size * 0.0425,
            Fill = new SolidColorBrush(Bar),
            Margin = new Thickness(0, 0, 0, size * 0.10),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        grid.Children.Add(bar);

        return grid;
    }
}
