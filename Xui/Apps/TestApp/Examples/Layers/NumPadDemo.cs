using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using L = Xui.Core.UI.Layers;

namespace Xui.Apps.TestApp.Examples.Layers;

/// <summary>
/// Phone-style dial pad using <see cref="L.UniformMonoGrid{TChild,TBuffer}"/> with
/// fixed-size keys, inset 20px from the frame edge.
/// </summary>
public class NumPadDemo
    : L.LayerView<L.Inset<L.UniformMonoGrid<NumKey, L.LayerBuffer16<NumKey>>>>
{
    private const int Pad  = 20;
    private const int Gap  = 8;
    private const int Cols = 3;
    private const int Rows = 5;

    private static readonly (string Label, string? Tag)[] KeyDefs =
    [
        ("1",  null), ("2",  null), ("3",  null),
        ("4",  null), ("5",  null), ("6",  null),
        ("7",  null), ("8",  null), ("9",  null),
        ("*",  null), ("0",  null), ("#",  null),
        ("",   ""),   ("C",  "CALL"), ("<", "DEL"),
    ];

    private static readonly Color BgKey        = new(0xFF, 0xFF, 0xFF, 0xFF);
    private static readonly Color BgKeyPressed  = new(0xCC, 0xD8, 0xFF, 0xFF);
    private static readonly Color BgCall        = new(0x28, 0xC7, 0x62, 0xFF);
    private static readonly Color BgCallPressed = new(0x1A, 0x96, 0x48, 0xFF);
    private static readonly Color FgDark        = new(0x1A, 0x1A, 0x1A, 0xFF);
    private static readonly Color FgLight       = new(0xFF, 0xFF, 0xFF, 0xFF);

    private int _pressedIdx = -1;

    public NumPadDemo()
    {
        Layer = new()
        {
            Value = Pad,
            Child = new()
            {
                Columns = Cols,
                Rows    = Rows,
                Gap     = Gap,
                Count   = KeyDefs.Length,
            },
        };

        for (int i = 0; i < KeyDefs.Length; i++)
        {
            var (label, tag) = KeyDefs[i];
            Layer.Child.Children[i] = new NumKey
            {
                Label           = label,
                FontFamily      = ["Inter"],
                FontSize        = 22,
                CornerRadius    = 10,
                BackgroundColor = tag switch { "" => default, "CALL" => BgCall, _ => BgKey },
                TextColor       = tag == "CALL" ? FgLight : FgDark,
            };
        }
    }

    // Sync background colors for all keys, reflecting the current pressed state.
    private void SyncColors()
    {
        for (int i = 0; i < KeyDefs.Length; i++)
        {
            var (_, tag) = KeyDefs[i];
            bool pressed = i == _pressedIdx;
            Layer.Child.Children[i].BackgroundColor = (tag, pressed) switch
            {
                ("",     _    ) => default,
                ("CALL", true ) => BgCallPressed,
                ("CALL", false) => BgCall,
                (_,      true ) => BgKeyPressed,
                _               => BgKey,
            };
        }
    }

    // Returns the key index under (px, py), or -1. Cell sizes are computed from the
    // inset grid area (Frame shrunk by Pad on all sides).
    private int HitIndex(NFloat px, NFloat py)
    {
        px -= this.Frame.X + Pad;
        py -= this.Frame.Y + Pad;
        if (px < 0 || py < 0) return -1;
        var gridW = this.Frame.Width  - Pad * 2;
        var gridH = this.Frame.Height - Pad * 2;
        var cellW = (gridW - Gap * (Cols - 1)) / Cols;
        var cellH = (gridH - Gap * (Rows - 1)) / Rows;
        int col = (int)(px / (cellW + Gap));
        int row = (int)(py / (cellH + Gap));
        if ((uint)col >= Cols || (uint)row >= Rows) return -1;
        if (px - col * (cellW + Gap) >= cellW) return -1;
        if (py - row * (cellH + Gap) >= cellH) return -1;
        return row * Cols + col;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.State.PointerType == PointerType.Mouse && phase == EventPhase.Tunnel)
        {
            if (e.Type == PointerEventType.Down)
            {
                int idx = HitIndex(e.State.Position.X, e.State.Position.Y);
                if (idx >= 0 && KeyDefs[idx].Tag != "")
                {
                    _pressedIdx = idx;
                    SyncColors();
                    this.CapturePointer(e.PointerId);
                    InvalidateRender();
                }
            }
            else if (e.Type == PointerEventType.Up)
            {
                if (_pressedIdx >= 0)
                {
                    int idx = HitIndex(e.State.Position.X, e.State.Position.Y);
                    if (idx == _pressedIdx)
                    {
                        // fire action (extend here later)
                    }
                    this.ReleasePointer(e.PointerId);
                    _pressedIdx = -1;
                    SyncColors();
                    InvalidateRender();
                }
            }
        }

        base.OnPointerEvent(ref e, phase);
    }
}

/// <summary>
/// A single dial-pad key: filled rounded rect with a centred label.
/// A transparent <see cref="BackgroundColor"/> renders nothing (empty cell).
/// </summary>
public struct NumKey : L.ILeaf
{
    public string? Label;
    public string[] FontFamily;
    public NFloat FontSize;
    public NFloat CornerRadius;
    public Color BackgroundColor;
    public Color TextColor;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
            guide.DesiredSize = guide.AvailableSize;

        if (guide.IsRender && !BackgroundColor.IsTransparent)
        {
            var ctx = guide.RenderContext!;
            var r   = guide.ArrangedRect;

            ctx.BeginPath();
            ctx.RoundRect(r, CornerRadius);
            ctx.SetFill(BackgroundColor);
            ctx.Fill();

            if (!TextColor.IsTransparent && !string.IsNullOrEmpty(Label))
            {
                ctx.SetFont(new Font { FontFamily = FontFamily, FontSize = FontSize });
                ctx.TextBaseline = TextBaseline.Middle;
                ctx.TextAlign    = TextAlign.Center;
                ctx.SetFill(TextColor);
                ctx.FillText(Label, new Point(r.X + r.Width / 2, r.Y + r.Height / 2));
            }
        }

        return guide;
    }
}
