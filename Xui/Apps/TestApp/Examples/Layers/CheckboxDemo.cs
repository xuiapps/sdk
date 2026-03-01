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
/// Demonstrates an interactive desktop-style checkbox using <c>DockLeft&lt;CheckBox, Inset&lt;Label&gt;&gt;</c>.
/// Clicking a row toggles its checked state.
/// </summary>
public class CheckboxDemo
    : L.LayerView<L.VerticalMonoStack<CheckRow, L.LayerBuffer4<CheckRow>>>
{
    private const int RowHeight = 36;
    private const int RowGap    = 12;

    private static CheckRow MakeRow(string text) => new()
    {
        Gap  = 10,
        Left = new L.CheckBox
        {
            Size            = 20,
            CornerRadius    = 4,
            StrokeWidth     = 2,
            BackgroundColor = White,
            BorderColor     = new Color(0x60, 0x60, 0x70, 0xFF),
            CheckColor      = new Color(0x10, 0x80, 0x40, 0xFF),
        },
        Right = new L.Inset<L.Label>
        {
            Value = ((NFloat)(RowHeight - 17) / 2, 0),
            Child = new L.Label
            {
                Text       = text,
                FontFamily = ["Inter"],
                FontSize   = 14,
                TextColor  = new Color(0x20, 0x20, 0x20, 0xFF),
            },
        },
    };

    public CheckboxDemo()
    {
        Layer = new() { Count = 3, Gap = RowGap };
        Layer.Children[0] = MakeRow("Enable dark mode");
        Layer.Children[1] = MakeRow("Show grid lines");
        Layer.Children[2] = MakeRow("Auto-save on exit");

        // Initial checked state set once:
        Layer.Children[0].Left.Checked = true;
        Layer.Children[2].Left.Checked = true;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse &&
            phase == EventPhase.Tunnel &&
            e.Type == PointerEventType.Up)
        {
            var py  = e.State.Position.Y;
            var top = this.Frame.Y;
            for (int i = 0; i < 3; i++)
            {
                var rowTop = top + i * (RowHeight + RowGap);
                if (py >= rowTop && py < rowTop + RowHeight)
                {
                    Layer.Children[i].Left.Checked = !Layer.Children[i].Left.Checked;
                    InvalidateRender();
                    break;
                }
            }
        }
        base.OnPointerEvent(ref e, phase);
    }
}
