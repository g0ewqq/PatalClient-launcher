using System.Windows;
using System.Windows.Controls;

namespace PatalClient.Launcher.UI;

public static class TextSpacing
{
    public static readonly DependencyProperty TrackingProperty = DependencyProperty.RegisterAttached(
        "Tracking", typeof(double), typeof(TextSpacing),
        new FrameworkPropertyMetadata(0.0, OnTrackingChanged));

    public static double GetTracking(TextBlock textBlock) => (double)textBlock.GetValue(TrackingProperty);
    public static void SetTracking(TextBlock textBlock, double value) => textBlock.SetValue(TrackingProperty, value);

    private static void OnTrackingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock)
            return;

        if (textBlock.Tag is not string original)
        {
            original = textBlock.Text;
            textBlock.Tag = original;
        }

        var tracking = (double)e.NewValue;
        if (tracking <= 0)
        {
            textBlock.Text = original;
            return;
        }

        var gap = (char)(0x2004 + Math.Clamp((int)tracking, 0, 2));
        textBlock.Text = string.Join(gap, original.ToCharArray());
    }
}
