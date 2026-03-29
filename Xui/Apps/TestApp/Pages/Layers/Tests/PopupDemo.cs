using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;

namespace Xui.Apps.TestApp.Pages.Layers.Tests;

/// <summary>
/// Demo for IPopup — shows a cross-platform in-window overlay anchored below a button.
/// </summary>
public class PopupDemo : View
{
    private static readonly NFloat ButtonH = 36;
    private static readonly NFloat ButtonW = 160;

    private bool hover;
    private bool pressed;
    private IPopup? popup;

    protected override Size MeasureCore(Size available, IMeasureContext ctx)
        => new Size(available.Width, available.Height);

    protected override void RenderCore(IContext ctx)
    {
        var rect = Frame;

        // Background
        ctx.SetFill(new Color(0xF5F5F5FF));
        ctx.FillRect(rect);

        // Center button
        var btnRect = ButtonRect();
        var bg = pressed ? new Color(0x0060C0FF)
               : hover   ? new Color(0x0084E8FF)
                         : new Color(0x0078D4FF);

        ctx.BeginPath();
        ctx.RoundRect(btnRect, 6);
        ctx.SetFill(bg);
        ctx.Fill();

        ctx.SetFont(new Font(14, ["Segoe UI"], fontWeight: pressed ? FontWeight.SemiBold : FontWeight.Normal));
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Middle;
        ctx.SetFill(Colors.White);
        ctx.FillText(popup?.IsVisible == true ? "Close Popup" : "Open Popup",
            new Point(btnRect.X + btnRect.Width / 2, btnRect.Y + btnRect.Height / 2));

        // Hint
        ctx.SetFont(new Font(12, ["Segoe UI"]));
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Top;
        ctx.SetFill(new Color(0x808080FF));
        ctx.FillText("IPopup — cross-platform overlay, auto-dismiss on outside click",
            new Point(rect.X + rect.Width / 2, btnRect.Y + btnRect.Height + 16));
    }

    private Rect ButtonRect()
    {
        var rect = Frame;
        NFloat x = rect.X + (rect.Width - ButtonW) / 2;
        NFloat y = rect.Y + (rect.Height - ButtonH) / 2 - 30;
        return new Rect(x, y, ButtonW, ButtonH);
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble) return;

        var btn = ButtonRect();
        bool inBtn = btn.Contains(e.State.Position);

        switch (e.Type)
        {
            case PointerEventType.Enter:
            case PointerEventType.Move:
                bool wasHover = hover;
                hover = inBtn;
                if (hover != wasHover) InvalidateRender();
                break;

            case PointerEventType.Leave:
                hover = false;
                pressed = false;
                InvalidateRender();
                break;

            case PointerEventType.Down when inBtn:
                pressed = true;
                CapturePointer(e.PointerId);
                InvalidateRender();
                break;

            case PointerEventType.Up when pressed:
                pressed = false;
                ReleasePointer(e.PointerId);
                if (inBtn) TogglePopup();
                InvalidateRender();
                break;
        }
    }

    private void TogglePopup()
    {
        if (popup?.IsVisible == true)
        {
            popup.Close();
            return;
        }

        if (popup == null)
        {
            popup = (IPopup)this.GetService(typeof(IPopup))!;
            popup.Closed += () => InvalidateRender();
        }

        var anchor = ButtonRect();
        popup.Show(new PopupContent(), anchor, PopupPlacement.Below, new Size(anchor.Width, 120));
        InvalidateRender();
    }

    /// <summary>Simple popup content — renders a card with some text inside the overlay.</summary>
    private class PopupContent : View
    {
        protected override Size MeasureCore(Size available, IMeasureContext ctx)
            => available;

        protected override void RenderCore(IContext ctx)
        {
            var rect = Frame;

            // Card background
            ctx.BeginPath();
            ctx.RoundRect(rect, 8);
            ctx.SetFill(new Color(0xFFFFFFFF));
            ctx.Fill();

            // Border
            ctx.BeginPath();
            ctx.RoundRect(rect, 8);
            ctx.SetStroke(new Color(0x0000001A));
            ctx.LineWidth = 1;
            ctx.Stroke();

            // Content
            ctx.SetFont(new Font(14, ["Segoe UI"], fontWeight: FontWeight.SemiBold));
            ctx.TextBaseline = TextBaseline.Top;
            ctx.TextAlign = TextAlign.Left;
            ctx.SetFill(new Color(0x1A1A1AFF));
            ctx.FillText("Popup content", new Point(rect.X + 16, rect.Y + 16));

            ctx.SetFont(new Font(13, ["Segoe UI"]));
            ctx.SetFill(new Color(0x606060FF));
            ctx.FillText("Cross-platform in-window overlay", new Point(rect.X + 16, rect.Y + 40));
            ctx.FillText("Works on Windows, macOS, iOS, Browser", new Point(rect.X + 16, rect.Y + 60));
        }
    }
}
