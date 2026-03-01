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
/// Six independent checkboxes stacked vertically using
/// <see cref="L.VerticalMonoStack{TChild,TBuffer}"/> with a
/// <see cref="L.LayerBuffer8{T}"/> backing buffer.
/// </summary>
public class VerticalStackDemo
    : L.LayerView<L.VerticalMonoStack<CheckRow, L.LayerBuffer8<CheckRow>>>
{
    private const int RowHeight = 36;
    private const int RowGap    = 8;

    private static readonly string[] Labels =
    [
        "Enable notifications",
        "Dark mode",
        "Auto-save on exit",
        "Show line numbers",
        "Compact view",
        "Send analytics",
    ];

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

    public VerticalStackDemo()
    {
        Layer = new() { Count = 6, Gap = RowGap };
        for (int i = 0; i < Labels.Length; i++)
            Layer.Children[i] = MakeRow(Labels[i]);

        // Initial checked state set once:
        Layer.Children[0].Left.Checked = true;
        Layer.Children[2].Left.Checked = true;
        Layer.Children[3].Left.Checked = true;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse &&
            phase == EventPhase.Tunnel &&
            e.Type == PointerEventType.Up)
        {
            var py  = e.State.Position.Y;
            var top = this.Frame.Y;
            for (int i = 0; i < 6; i++)
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
