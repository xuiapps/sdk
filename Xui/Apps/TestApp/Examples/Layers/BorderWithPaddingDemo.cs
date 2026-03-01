using Xui.Core.Canvas;
using Xui.Core.Math2D;
using L = Xui.Core.UI.Layers;

// C# 12 generic type aliases for the two distinct row types.
using NoPadRow  = Xui.Core.UI.Layers.Border<Xui.Core.UI.Layers.Label>;
using PaddedRow = Xui.Core.UI.Layers.Border<Xui.Core.UI.Layers.Inset<Xui.Core.UI.Layers.Label>>;

namespace Xui.Apps.TestApp.Examples.Layers;

/// <summary>
/// Demonstrates how composing <c>Inset&lt;T&gt;</c> inside <c>Border&lt;T&gt;</c> adds padding.
/// Shows the same border with no padding, uniform padding, and asymmetric padding.
/// </summary>
public class BorderWithPaddingDemo
    : L.LayerView<L.VerticalPolyStack<NoPadRow, PaddedRow, PaddedRow, L.Empty, L.Empty, L.Empty, L.Empty, L.Empty>>
{
    public BorderWithPaddingDemo()
    {
        Layer = new() { Gap = 20 };

        // No padding: text sits directly against the border edge
        Layer.Item1 = new()
        {
            BorderThickness = 2,
            CornerRadius = 6,
            BorderColor = new Color(0x80, 0x80, 0x80, 0xFF),
            BackgroundColor = new Color(0xF0, 0xF0, 0xF0, 0xFF),
            Child = new L.Label
            {
                Text = "No padding (Border<Label>)",
                FontFamily = ["Inter"],
                FontSize = 13,
                TextColor = new Color(0x30, 0x30, 0x30, 0xFF),
            },
        };

        // Uniform padding: Inset(10) inside the border
        Layer.Item2 = new()
        {
            BorderThickness = 2,
            CornerRadius = 6,
            BorderColor = new Color(0x20, 0x80, 0xC0, 0xFF),
            BackgroundColor = new Color(0xD0, 0xE8, 0xFF, 0xFF),
            Child = new L.Inset<L.Label>
            {
                Value = 10,
                Child = new L.Label
                {
                    Text = "Uniform padding (Border<Inset<Label>>)",
                    FontFamily = ["Inter"],
                    FontSize = 13,
                    TextColor = new Color(0x10, 0x40, 0x80, 0xFF),
                },
            },
        };

        // Asymmetric padding: more left/right than top/bottom
        Layer.Item3 = new()
        {
            BorderThickness = 2,
            CornerRadius = 6,
            BorderColor = new Color(0x80, 0x40, 0xC0, 0xFF),
            BackgroundColor = new Color(0xEA, 0xD8, 0xFF, 0xFF),
            Child = new L.Inset<L.Label>
            {
                Value = new Frame(top: 6, right: 24, bottom: 6, left: 24),
                Child = new L.Label
                {
                    Text = "Asymmetric padding (top:6 side:24)",
                    FontFamily = ["Inter"],
                    FontSize = 13,
                    TextColor = new Color(0x40, 0x10, 0x70, 0xFF),
                },
            },
        };
    }
}
