using Xui.Core.Canvas;
using L = Xui.Core.UI.Layers;

namespace Xui.Apps.TestApp.Examples.Layers;

/// <summary>
/// Demonstrates the <c>Border&lt;T&gt;</c> layer with several visual configurations.
/// Each row shows a different border + label composition rendered via LayoutGuide.
/// </summary>
public class BorderDemo
    : L.LayerView<L.VerticalMonoStack<L.Border<L.Label>, L.LayerBuffer4<L.Border<L.Label>>>>
{
    public BorderDemo()
    {
        Layer = new() { Count = 4, Gap = 24 };

        // 1. Simple outline border — no background, sharp corners
        Layer.Children[0] = new()
        {
            BorderThickness = 2,
            BorderColor = new Color(0x30, 0x30, 0x30, 0xFF),
            Child = new L.Label
            {
                Text = "Sharp border, no background",
                FontFamily = ["Inter"],
                FontSize = 14,
                TextColor = new Color(0x30, 0x30, 0x30, 0xFF),
            },
        };

        // 2. Filled border — background color + border line, sharp corners
        Layer.Children[1] = new()
        {
            BorderThickness = 2,
            BorderColor = new Color(0x20, 0x80, 0xC0, 0xFF),
            BackgroundColor = new Color(0xD0, 0xE8, 0xFF, 0xFF),
            Child = new L.Label
            {
                Text = "Sharp border + background",
                FontFamily = ["Inter"],
                FontSize = 14,
                TextColor = new Color(0x10, 0x40, 0x80, 0xFF),
            },
        };

        // 3. Rounded border — background + rounded corners
        Layer.Children[2] = new()
        {
            BorderThickness = 2,
            CornerRadius = 10,
            BorderColor = new Color(0x50, 0xA0, 0x30, 0xFF),
            BackgroundColor = new Color(0xD8, 0xF4, 0xD0, 0xFF),
            Child = new L.Label
            {
                Text = "Rounded border + background",
                FontFamily = ["Inter"],
                FontSize = 14,
                TextColor = new Color(0x20, 0x60, 0x10, 0xFF),
            },
        };

        // 4. Background only — no border line, pill-shaped
        Layer.Children[3] = new()
        {
            BorderThickness = 0,
            CornerRadius = 14,
            BackgroundColor = new Color(0xFF, 0xE0, 0x80, 0xFF),
            Child = new L.Label
            {
                Text = "Background only, no border",
                FontFamily = ["Inter"],
                FontSize = 14,
                TextColor = new Color(0x60, 0x40, 0x00, 0xFF),
            },
        };
    }
}
