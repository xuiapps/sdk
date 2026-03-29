using System;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// Cross-platform popup implementation rendered as an in-window overlay inside <see cref="RootView"/>.
/// Works on every platform (Windows, macOS, iOS, Android, Browser) with no platform-specific code.
/// Acquired via <c>GetService&lt;IPopup&gt;()</c> from any <see cref="View"/>.
/// </summary>
internal sealed class PopupOverlay : IPopup
{
    private readonly RootView rootView;

    internal View? Content { get; private set; }
    internal Rect Frame { get; private set; }

    public bool IsVisible { get; private set; }
    public event Action? Closed;

    internal PopupOverlay(RootView rootView)
    {
        this.rootView = rootView;
    }

    public void Show(View content, Rect anchorRect, PopupPlacement placement = PopupPlacement.Below, Size? size = null, PopupEffect effect = PopupEffect.None)
    {
        if (IsVisible) Close();

        Content = content;
        var popupSize = size ?? new Size(anchorRect.Width, 120);
        Frame = ComputeFrame(anchorRect, placement, popupSize, rootView.Frame);
        IsVisible = true;
        rootView.AddOverlay(this);
    }

    public void Close()
    {
        if (!IsVisible) return;
        IsVisible = false;
        rootView.RemoveOverlay(this);
        Closed?.Invoke();
        Content = null;
        Frame = default;
    }

    public void Dispose() => Close();

    /// <summary>Renders this overlay's content into the window via a LayoutGuide, clipped to its frame.</summary>
    internal void Render(LayoutGuide parentGuide)
    {
        if (Content == null) return;

        var ctx = parentGuide.RenderContext;
        ctx?.Save();
        ctx?.BeginPath();
        ctx?.Rect(Frame);
        ctx?.Clip();

        Content.Update(new LayoutGuide
        {
            Anchor = Frame.TopLeft,
            PreviousTime = parentGuide.PreviousTime,
            CurrentTime = parentGuide.CurrentTime,
            Pass =
                LayoutGuide.LayoutPass.Measure |
                LayoutGuide.LayoutPass.Arrange |
                LayoutGuide.LayoutPass.Render,
            AvailableSize = Frame.Size,
            XAlign = LayoutGuide.Align.Start,
            YAlign = LayoutGuide.Align.Start,
            XSize = LayoutGuide.SizeTo.Exact,
            YSize = LayoutGuide.SizeTo.Exact,
            MeasureContext = parentGuide.MeasureContext,
            RenderContext = parentGuide.RenderContext,
            Instruments = parentGuide.Instruments,
        });

        ctx?.Restore();
    }

    /// <summary>Returns true if the point is inside this popup's frame.</summary>
    internal bool ContainsPoint(Point p) => Frame.Contains(p);

    private static Rect ComputeFrame(Rect anchor, PopupPlacement placement, Size size, Rect windowFrame)
    {
        nfloat x, y;
        switch (placement)
        {
            case PopupPlacement.Above:
                x = anchor.X;
                y = anchor.Y - size.Height;
                break;
            case PopupPlacement.Right:
                x = anchor.X + anchor.Width;
                y = anchor.Y;
                break;
            case PopupPlacement.Left:
                x = anchor.X - size.Width;
                y = anchor.Y;
                break;
            default: // Below
                x = anchor.X;
                y = anchor.Y + anchor.Height;
                break;
        }

        // Clamp to stay within the window
        x = nfloat.Clamp(x, windowFrame.X, windowFrame.X + windowFrame.Width - size.Width);
        y = nfloat.Clamp(y, windowFrame.Y, windowFrame.Y + windowFrame.Height - size.Height);

        return new Rect(x, y, size.Width, size.Height);
    }
}
