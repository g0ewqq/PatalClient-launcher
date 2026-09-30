using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PatalClient.Launcher.UI;

public static class PatalLogo
{
    private static readonly Color InkColor = Color.FromRgb(0xD5, 0xD8, 0xDC);

    private static Path Monogram(double size, double stroke, Color color)
    {
        var bcx = size * 0.545;
        var bcy = size * 0.355;
        var r = size * 0.265;
        var stemX = bcx - r;
        var stemBottom = size * 0.91;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(bcx, bcy - r), false, false);
            ctx.ArcTo(new Point(bcx, bcy + r), new Size(r, r), 0, true,
                SweepDirection.Clockwise, true, false);
            ctx.ArcTo(new Point(bcx, bcy - r), new Size(r, r), 0, true,
                SweepDirection.Clockwise, true, false);

            ctx.BeginFigure(new Point(stemX, bcy), false, false);
            ctx.LineTo(new Point(stemX, stemBottom), true, false);
        }

        return new Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = stroke,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Stretch = Stretch.None
        };
    }

    public static FrameworkElement CreateMark(double size = 15)
    {
        return Monogram(size, Math.Max(1.4, size * 0.11), InkColor);
    }

    public static FrameworkElement CreateHero(double size = 76)
    {
        var grid = new Grid { Width = size, Height = size };
        grid.Children.Add(Monogram(size, Math.Max(2, size * 0.075), InkColor));

        grid.Children.Add(new Ellipse
        {
            Width = size * 0.11,
            Height = size * 0.11,
            Fill = new SolidColorBrush(Color.FromRgb(0x53, 0x78, 0xB0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(size * 0.09, 0, 0, 0)
        });

        return grid;
    }
}
