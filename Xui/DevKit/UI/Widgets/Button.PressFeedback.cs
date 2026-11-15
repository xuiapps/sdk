using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.DevKit.UI.Layer;

namespace Xui.DevKit.UI.Widgets;

public partial class Button
{
    private PressFeedbackLayer<Button> pressFeedback;
    private int pressedPointer = -1;

    /// <inheritdoc/>
    protected override void OnActivate()
    {
        base.OnActivate();
        pressFeedback.Activate(this);
    }

    private void BeginPress(int pointer, Point position, bool delay)
    {
        pressFeedback.SetBounds(Frame);
        pressFeedback.Begin(this, position, delay);
        pressedPointer = pointer;
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
        pressFeedback.Animate(this, previous, current);
    }

    private static Color Blend(Color a, Color b, nfloat t) => new(
        a.Red + (b.Red - a.Red) * t, a.Green + (b.Green - a.Green) * t,
        a.Blue + (b.Blue - a.Blue) * t, a.Alpha + (b.Alpha - a.Alpha) * t);
}
