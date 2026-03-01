using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.UI.Input;
using L = Xui.Core.UI.Layers;
using static Xui.Core.Canvas.Colors;

// C# 12 generic type alias — keeps the class declaration readable.
using CheckRow = Xui.Core.UI.Layers.DockLeft<
    Xui.Core.UI.Layers.CheckBox,
    Xui.Core.UI.Layers.Inset<Xui.Core.UI.Layers.Label>>;

namespace Xui.Apps.TestApp.Examples.Layers;

/// <summary>
/// Demonstrates <see cref="L.VerticalPolyStack{T1,T2,T3,T4,T5,T6,T7,T8}"/> with two
/// heterogeneous section headings (<see cref="L.Label"/>) interleaved with five
/// checkbox rows (<see cref="CheckRow"/>), hosted in a <see cref="L.LayerView{T}"/>.
/// </summary>
public class VerticalPolyStackDemo
    : L.LayerView<L.VerticalPolyStack<L.Label, CheckRow, CheckRow, L.Label, CheckRow, CheckRow, CheckRow, L.Empty>>
{
    private const int HeadingH = 20;
    private const int RowH = 36;
    private const int Gap = 8;

    private static int CheckboxTopY(int i) => i switch
    {
        0 => HeadingH + Gap,
        1 => HeadingH + Gap + RowH + Gap,
        2 => HeadingH + Gap + (RowH + Gap) * 2 + HeadingH + Gap,
        3 => HeadingH + Gap + (RowH + Gap) * 2 + HeadingH + Gap + RowH + Gap,
        4 => HeadingH + Gap + (RowH + Gap) * 2 + HeadingH + Gap + (RowH + Gap) * 2,
        _ => -1
    };

    private static L.Label MakeHeading(string text) => new()
    {
        Text = text,
        FontFamily = ["Inter"],
        FontSize = 13,
        FontWeight = Xui.Core.Canvas.FontWeight.Bold,
        TextColor = 0x444455FF,
    };

    private static CheckRow MakeRow(string text) => new()
    {
        Gap  = 10,
        Left = new L.CheckBox
        {
            Size = 20,
            CornerRadius = 4,
            StrokeWidth = 2,
            BackgroundColor = White,
            BorderColor = 0x606070FF,
            CheckColor = 0x108040FF,
        },
        Right = new L.Inset<L.Label>
        {
            Value = ((NFloat)(RowH - 17) / 2, 0),
            Child = new L.Label
            {
                Text = text,
                FontFamily = ["Inter"],
                FontSize = 14,
                TextColor = new Color(0x20, 0x20, 0x20, 0xFF),
            },
        },
    };

    // VerticalPolyStack slots:
    // Item1 = Label       (heading "Display")
    // Item2 = CheckRow    (Dark mode)
    // Item3 = CheckRow    (Show grid lines)
    // Item4 = Label       (heading "Privacy")
    // Item5 = CheckRow    (Send analytics)
    // Item6 = CheckRow    (Location access)
    // Item7 = CheckRow    (Camera access)
    // Item8 = Empty       (unused)
    public VerticalPolyStackDemo()
    {
        Layer = new() { Gap = Gap };
        Layer.Item1 = MakeHeading("Display");
        Layer.Item2 = MakeRow("Dark mode");
        Layer.Item3 = MakeRow("Show grid lines");
        Layer.Item4 = MakeHeading("Privacy");
        Layer.Item5 = MakeRow("Send analytics");
        Layer.Item6 = MakeRow("Location access");
        Layer.Item7 = MakeRow("Camera access");
        // Item8 is Empty — default(L.Empty), no assignment needed

        // Initial checked state set once:
        Layer.Item2.Left.Checked = true;
        Layer.Item6.Left.Checked = true;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse &&
            phase == EventPhase.Tunnel &&
            e.Type == PointerEventType.Up)
        {
            var relY = e.State.Position.Y - Frame.Y;
            for (int i = 0; i < 5; i++)
            {
                var top = CheckboxTopY(i);
                if (relY >= top && relY < top + RowH)
                {
                    switch (i)
                    {
                        case 0: Layer.Item2.Left.Checked = !Layer.Item2.Left.Checked; break;
                        case 1: Layer.Item3.Left.Checked = !Layer.Item3.Left.Checked; break;
                        case 2: Layer.Item5.Left.Checked = !Layer.Item5.Left.Checked; break;
                        case 3: Layer.Item6.Left.Checked = !Layer.Item6.Left.Checked; break;
                        case 4: Layer.Item7.Left.Checked = !Layer.Item7.Left.Checked; break;
                    }
                    InvalidateRender();
                    break;
                }
            }
        }
        base.OnPointerEvent(ref e, phase);
    }
}
