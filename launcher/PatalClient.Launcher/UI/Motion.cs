using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PatalClient.Launcher.UI;

public static class Motion
{
    public static bool ReducedMotion { get; set; }

    public static readonly Duration Fast = new(TimeSpan.FromMilliseconds(130));
    public static readonly Duration Entrance = new(TimeSpan.FromMilliseconds(450));

    public static IEasingFunction EaseOut { get; } = new CubicEase { EasingMode = EasingMode.EaseOut };

    public static void FadeScale(FrameworkElement element, double scaleFrom, Duration duration)
    {
        if (ReducedMotion)
            return;

        var transform = new ScaleTransform(scaleFrom, scaleFrom);
        element.RenderTransform = transform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        element.Opacity = 0;

        var storyboard = new Storyboard();
        var fadeIn = new DoubleAnimation(0, 1, duration) { EasingFunction = EaseOut };
        var scaleTo = new DoubleAnimation(scaleFrom, 1, duration) { EasingFunction = EaseOut };

        Storyboard.SetTarget(fadeIn, element);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
        Storyboard.SetTarget(scaleTo, element);
        Storyboard.SetTargetProperty(scaleTo, new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(fadeIn);
        storyboard.Children.Add(scaleTo);

        var scaleY = scaleTo.Clone();
        Storyboard.SetTarget(scaleY, element);
        Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));
        storyboard.Children.Add(scaleY);

        storyboard.Begin(element);
    }
}
