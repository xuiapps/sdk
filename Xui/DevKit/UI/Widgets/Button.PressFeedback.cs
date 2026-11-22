using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.DevKit.UI.Layer;

namespace Xui.DevKit.UI.Widgets;

public partial class Button
{
    private PressFeedbackLayer<Button> pressFeedback;
    private int pressedPointer = -1;
    private bool maybeRaised, highlightRendered, clickPending, raisingFarFetch;

    private void RaiseClickMaybe()
    {
        if (maybeRaised) return;
        maybeRaised = true;
        ClickMaybe?.Invoke();
    }

    private void CommitClick()
    {
        RaiseClickMaybe(); // A quick release bypasses the recognition delay.
        clickPending = true;
        raisingFarFetch = true;
        try { ClickFarFetch?.Invoke(); }
        finally { raisingFarFetch = false; }
        if (!clickPending) return;
        if (!WaitForPressFeedback || highlightRendered || (Flags & ViewFlags.Active) == 0) CompleteClick();
        else { InvalidateRender(); RequestAnimationFrame(); }
    }

    private void CompleteClick()
    {
        clickPending = false;
        Click?.Invoke();
        Clicked?.Invoke();
    }

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        base.OnActivate();
        pressFeedback.Activate(this);
    }

    private void BeginPress(int pointer, Point position, bool delay)
    {
        maybeRaised = highlightRendered = false;
        pressFeedback.SetBounds(Frame);
        pressFeedback.Begin(this, position, delay);
        pressedPointer = pointer;
        if (pressFeedback.IsPressVisible) RaiseClickMaybe();
    }

    private void EndPress(bool tapped)
    {
        pressedPointer = -1; // ReleasePointer synchronously sends LostCapture.
        pressFeedback.End(this, tapped);
    }

    private nfloat FeedbackOpacity => pressFeedback.FeedbackOpacity;

    private void RenderRipples(IContext context)
    {
        pressFeedback.SetBounds(Frame);
        pressFeedback.CornerRadius = cornerRadius;
        pressFeedback.RippleColor = textColor;
        pressFeedback.Render(this, context);
    }

    /// <inheritdoc/>
    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        base.AnimateCore(previous, current);
        if (clickPending && !highlightRendered) pressFeedback.HoldReleasedFeedback();
        pressFeedback.Animate(this, previous, current);
        if (pressedPointer != -1 && pressFeedback.IsPressVisible) RaiseClickMaybe();
        if (clickPending)
        {
            if (highlightRendered) CompleteClick();
            else { InvalidateRender(); RequestAnimationFrame(); }
        }
    }

    private static Color Blend(Color a, Color b, nfloat t) => new(
        a.Red + (b.Red - a.Red) * t, a.Green + (b.Green - a.Green) * t,
        a.Blue + (b.Blue - a.Blue) * t, a.Alpha + (b.Alpha - a.Alpha) * t);
}
