using Xui.Core.Canvas;
using Xui.Core.UI;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Layout;

internal static class LayoutPreviewStyle
{
    public static readonly Color Ink = new(0x252B3AFF);
    public static readonly Color Muted = new(0x727B8DFF);
    public static readonly Color Blue = new(0x5266DEFF);
    public static readonly Color Teal = new(0x208B80FF);
    public static readonly Color PaleBlue = new(0xE9EDFCFF);
    public static readonly Color PaleTeal = new(0xE1F3EDFF);
    public static readonly Color PaleRose = new(0xFBE7ECFF);

    public static Label Text(string text, NFloat size = default, FontWeight? weight = null, Color? color = null) => new()
    {
        Text = text, FontFamily = "Inter", FontSize = size == 0 ? 14 : size,
        FontWeight = weight ?? FontWeight.Normal, TextColor = color ?? Ink,
    };

    public static Border Surface(View content, Color color, NFloat padding = default) => new()
    {
        BackgroundColor = color, CornerRadius = 14, Padding = padding == 0 ? 16 : padding, Content = content,
    };

    public static Border Chip(string text, Color color)
    {
        var label = Text(text, 13, FontWeight.SemiBold);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Middle;
        return new Border { Padding = (10, 14), CornerRadius = 12, BackgroundColor = color, Content = label };
    }

    public static Label Heading(string text)
    {
        var label = Text(text, 11, FontWeight.SemiBold, Teal);
        label.Margin = (0, 0, 18, 0);
        return label;
    }
}
