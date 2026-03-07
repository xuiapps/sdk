using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layer;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Layers.Tests;

/// <summary>
/// Demonstrates <see cref="BorderLayer{TView,TChild}"/> used directly inside a
/// <see cref="LayerView{TView,TLayer}"/>. Each box is a <c>LayerView&lt;View, BorderLayer&lt;View, ContentLayer&gt;&gt;</c>
/// with no child view — the layer handles all decoration.
/// </summary>
public class BorderLayerTest : View
{
    private readonly BoxView[] boxes;

    public override int Count => boxes.Length;
    public override View this[int index] => boxes[index];

    public BorderLayerTest()
    {
        boxes =
        [
            new BoxView
            {
                BackgroundColor = new Color(0x42, 0x85, 0xF4, 0xFF),
            },
            new BoxView
            {
                BorderColor = Black,
                BorderThickness = 2,
            },
            new BoxView
            {
                BackgroundColor = new Color(0x34, 0xA8, 0x53, 0xFF),
                BorderColor = new Color(0x0F, 0x9D, 0x58, 0xFF),
                BorderThickness = 3,
                CornerRadius = 12,
            },
            new BoxView
            {
                BackgroundColor = new Color(0xFF, 0xAA, 0x00, 0xFF),
                CornerRadius = 40,
            },
            new BoxView
            {
                BackgroundColor = new Color(0xEA, 0x43, 0x35, 0xFF),
                BorderColor = new Color(0xC5, 0x22, 0x1F, 0xFF),
                BorderThickness = 4,
                CornerRadius = 8,
            },
        ];

        foreach (var box in boxes)
            AddProtectedChild(box);
    }

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        foreach (var box in boxes)
            box.Measure((120, 80), context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat x = rect.X + 20;
        NFloat y = rect.Y + 20;

        foreach (var box in boxes)
        {
            box.Arrange(new Rect(x, y, 120, 80), context);
            x += 140;
        }
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(new Color(0xF8, 0xF9, 0xFA, 0xFF));
        context.FillRect(Frame);
        base.RenderCore(context);
    }

    // BoxView
    // A LayerView<View, BorderLayer<View, ContentLayer>> with no child view.
    // The layer handles all decoration; the view contributes nothing beyond size.
    private class BoxView : LayerView<View, BorderLayer<View, ContentLayer>>
    {
        public Color BackgroundColor
        {
            get => Layer.BackgroundColor;
            set => Layer.BackgroundColor = value;
        }

        public Color BorderColor
        {
            get => Layer.BorderColor;
            set => Layer.BorderColor = value;
        }

        public Frame BorderThickness
        {
            get => Layer.BorderThickness;
            set => Layer.BorderThickness = value;
        }

        public CornerRadius CornerRadius
        {
            get => Layer.CornerRadius;
            set => Layer.CornerRadius = value;
        }
    }
}
