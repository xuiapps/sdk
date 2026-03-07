using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Core.UI.Layer;

namespace Xui.Apps.TestApp.Pages.Layers.Tests;

/// <summary>
/// Demo-only leaf layer that renders a clickable button with an optional label,
/// margin, corner radius, and hover/pressed visual states.
/// When <see cref="Visible"/> is false, <see cref="Measure"/> returns zero width
/// so the docked slot collapses.
/// <typeparam name="THost">The host view type. Receives typed access in <typeparamref name="TAction"/>.</typeparam>
/// <typeparam name="TAction">Zero-allocation click handler struct.</typeparam>
/// </summary>
public struct ButtonLayer<THost, TAction> : ILayer<THost>
    where THost  : ILayerHost
    where TAction : struct, IButtonAction<THost>
{
    /// <summary>Label text drawn centered in the button.</summary>
    public string? Label;

    /// <summary>Outer margin: shrinks the visible button rect within the docked slot.</summary>
    public NFloat Margin;

    /// <summary>Corner radius of the visible button rect.</summary>
    public CornerRadius CornerRadius;

    public Color NormalColor;
    public Color HoverColor;
    public Color PressedColor;
    public Color LabelColor;
    public string[]? FontFamily;
    public NFloat FontSize;

    /// <summary>
    /// When false, <see cref="Measure"/> returns zero width so the dock slot collapses.
    /// </summary>
    public bool Visible;

    /// <summary>Zero-allocation click handler. Embed state here if needed.</summary>
    public TAction Action;

    private bool hover;
    private bool pressed;
    private Rect frame;

    // ── ILayer<THost> ────────────────────────────────────────────────────

    public void Update(THost view, ref LayoutGuide guide)
    {
        if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
        if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
        if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
        if (guide.IsRender)  Render(view, guide.RenderContext!);
    }

    public Size Measure(THost view, Size available, IMeasureContext ctx)
    {
        if (!Visible) return new Size(0, available.Height);
        NFloat side = NFloat.IsFinite(available.Height) ? available.Height
                    : FontSize > 0 ? FontSize * 2 : (NFloat)30;
        return new Size(side, side);
    }

    public void Arrange(THost view, Rect rect, IMeasureContext ctx)
    {
        frame = rect;
    }

    public void Render(THost view, IContext ctx)
    {
        if (!Visible) return;

        var btn = BtnRect();
        var bg = pressed ? PressedColor : hover ? HoverColor : NormalColor;

        if (!bg.IsTransparent)
        {
            if (CornerRadius.IsZero)
            {
                ctx.SetFill(bg);
                ctx.FillRect(btn);
            }
            else
            {
                ctx.BeginPath();
                ctx.RoundRect(btn, CornerRadius);
                ctx.SetFill(bg);
                ctx.Fill();
            }
        }

        if (!string.IsNullOrEmpty(Label))
        {
            ctx.SetFont(new Font
            {
                FontFamily = FontFamily ?? ["Inter"],
                FontSize   = FontSize > 0 ? FontSize : 13,
                FontWeight = FontWeight.Normal,
            });
            ctx.TextAlign    = TextAlign.Center;
            ctx.TextBaseline = TextBaseline.Middle;
            ctx.SetFill(LabelColor);
            ctx.FillText(Label, new Point(btn.X + btn.Width / 2, btn.Y + btn.Height / 2));
        }
    }

    public void Animate(THost view, TimeSpan p, TimeSpan c) { }

    public void OnPointerEvent(THost view, ref PointerEventRef e, EventPhase phase)
    {
        if (!Visible || phase != EventPhase.Bubble)
            return;

        var btn    = BtnRect();
        var pos    = e.State.Position;
        bool inBtn = pos.X >= btn.X && pos.X < btn.X + btn.Width
                  && pos.Y >= btn.Y && pos.Y < btn.Y + btn.Height;

        switch (e.Type)
        {
            case PointerEventType.Enter:
                hover = inBtn;
                view.InvalidateRender();
                break;

            case PointerEventType.Leave:
                hover   = false;
                pressed = false;
                view.InvalidateRender();
                break;

            case PointerEventType.Move:
                bool was = hover;
                hover = inBtn;
                if (hover != was) view.InvalidateRender();
                break;

            case PointerEventType.Down when inBtn:
                pressed = true;
                view.CapturePointer(e.PointerId);
                view.InvalidateRender();
                break;

            case PointerEventType.Up when pressed:
                pressed = false;
                view.ReleasePointer(e.PointerId);
                if (inBtn) Action.Execute(view);
                view.InvalidateRender();
                break;
        }
    }

    public void OnKeyDown(THost view, ref KeyEventRef e) { }
    public void OnChar(THost view, ref KeyEventRef e)    { }
    public void OnFocus(THost view)                      { }
    public void OnBlur(THost view)                       { }

    private Rect BtnRect() => new Rect(
        frame.X + Margin,
        frame.Y + Margin,
        frame.Width  - Margin * 2,
        frame.Height - Margin * 2);
}
