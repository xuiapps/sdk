using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests;

/// <summary>Retains the rendered image while navigation composes the moving page.</summary>
public abstract class CubeSurface : View
{
    private static readonly NFloat MaxTilt = NFloat.Pi * 70 / 180;
    private static readonly NFloat RadiansPerPoint = (NFloat)0.01;
    private static readonly NFloat MinimumVelocity = (NFloat)0.02;
    protected NFloat rotationX = 0.15f; // Pitch, limited to +/-70 degrees.
    protected NFloat rotationY = 0.3f;  // Yaw, free to turn through any number of revolutions.
    private int pointer = -1;
    private Point lastPoint;
    private NFloat sampledPitch, sampledYaw, pitchVelocity, yawVelocity;
    private TimeSpan? sampleTime;
    private bool releasePending;
    private ExponentialDecayCurve? pitchCoast, yawCoast;
    private IImage? image;
    private byte[]? pixels;
    private bool hasFrame, hasImage, dirty = true, isLive = true;

    public bool IsLive
    {
        get => isLive;
        set
        {
            if (isLive == value) return;
            isLive = value;
            if (!value) StopMotion();
            if (value && dirty) InvalidateRender();
        }
    }

    protected long SceneVersion { get; private set; }

    // Native and portable output have separate caches. A newly presented native frame
    // makes any previous readback image stale, including captures during a transition.
    protected void NativeSceneRendered() { hasFrame = false; dirty = false; }

    protected abstract string BackendLabel { get; }
    protected abstract bool RenderPixels(byte[] pixels);

    /// <summary>Marks scene changes without discarding the image used during a transition.</summary>
    protected void InvalidateScene()
    {
        SceneVersion++;
        dirty = true;
        if (IsLive) InvalidateRender();
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (!IsLive || phase != EventPhase.Bubble) return;
        if (e.Type == PointerEventType.Down && pointer == -1)
        {
            StopMotion();
            pointer = e.PointerId;
            lastPoint = e.State.Position;
            sampledPitch = rotationX; sampledYaw = rotationY;
            CapturePointer(pointer, PointerGestures.Drag);
            RequestAnimationFrame();
        }
        else if (pointer == e.PointerId)
        {
            if (e.Type == PointerEventType.Move)
                DragTo(e.State.Position);
            else if (e.Type == PointerEventType.Up)
            {
                DragTo(e.State.Position);
                var id = pointer; pointer = -1;
                releasePending = true;
                RequestAnimationFrame();
                ReleasePointer(id);
            }
            else if (e.Type is PointerEventType.Cancel or PointerEventType.LostCapture)
                StopMotion();
        }
    }

    private void DragTo(Point point)
    {
        var delta = point - lastPoint;
        lastPoint = point;
        // Incremental movement lets a finger reverse immediately at a tilt limit.
        SetRotation(rotationX - delta.Y * RadiansPerPoint, rotationY - delta.X * RadiansPerPoint);
    }

    private void SetRotation(NFloat pitch, NFloat yaw)
    {
        pitch = NFloat.Clamp(pitch, -MaxTilt, MaxTilt);
        if (pitch == rotationX && yaw == rotationY) return;
        rotationX = pitch; rotationY = yaw;
        InvalidateScene();
    }

    protected override void AnimateCore(TimeSpan previous, TimeSpan current)
    {
        if (!IsLive) return;
        if (pointer != -1 || releasePending)
        {
            // Like the temperature ruler, use the View frame clock to accumulate
            // filtered velocity, independently of native pointer-event frequency.
            var dt = (NFloat)(current - (sampleTime ?? previous)).TotalSeconds;
            if (releasePending && dt <= 0) { RequestAnimationFrame(); return; }
            if (dt > 0)
            {
                var weight = 1 - NFloat.Exp(-dt / (NFloat)0.04);
                pitchVelocity += (NFloat.Clamp((rotationX - sampledPitch) / dt, -12, 12) - pitchVelocity) * weight;
                yawVelocity += (NFloat.Clamp((rotationY - sampledYaw) / dt, -12, 12) - yawVelocity) * weight;
                sampledPitch = rotationX; sampledYaw = rotationY;
            }
            sampleTime = current;
            if (pointer != -1) { RequestAnimationFrame(); return; }
            releasePending = false;
            if (NFloat.Abs(pitchVelocity) > MinimumVelocity)
                pitchCoast = new(current, rotationX, pitchVelocity, ExponentialDecayCurve.Fast, (double)MinimumVelocity);
            if (NFloat.Abs(yawVelocity) > MinimumVelocity)
                yawCoast = new(current, rotationY, yawVelocity, ExponentialDecayCurve.Fast, (double)MinimumVelocity);
        }

        var pitch = rotationX;
        var yaw = rotationY;
        if (pitchCoast is { } vertical)
        {
            pitch = vertical[current];
            if (current >= vertical.EndTime || pitch <= -MaxTilt || pitch >= MaxTilt) pitchCoast = null;
        }
        if (yawCoast is { } horizontal)
        {
            yaw = horizontal[current];
            if (current >= horizontal.EndTime) yawCoast = null;
        }
        SetRotation(pitch, yaw);
        if (pitchCoast is not null || yawCoast is not null) RequestAnimationFrame();
    }

    private void StopMotion()
    {
        var id = pointer; pointer = -1;
        pitchCoast = yawCoast = null;
        releasePending = false; sampleTime = null;
        pitchVelocity = yawVelocity = 0;
        if (id != -1) ReleasePointer(id);
    }

    protected override void OnDeactivate()
    {
        StopMotion();
        base.OnDeactivate();
    }

    protected override Size MeasureCore(Size available, IMeasureContext context) => (300, 300);

    protected override void RenderCore(IContext context)
    {
        // Generate the first image even if navigation is already in progress.
        // Afterwards neither rendering, readback nor upload runs while frozen.
        if (!hasFrame || IsLive && dirty)
        {
            pixels ??= new byte[300 * 300 * 4];
            image ??= GetService(typeof(IImage)) as IImage;
            hasImage = RenderPixels(pixels);
            if (hasImage) image?.LoadPixels(300, 300, pixels);
            hasFrame = true;
            dirty = false;
        }

        var rect = new Rect(Frame.X, Frame.Y, 300, 300);
        if (hasImage && image is not null) context.DrawImage(image, rect);
        context.SetStroke(Colors.Black);
        context.StrokeRect(rect);
        context.SetFill(new Color(0x000000CC));
        context.SetFont(new Font(11, "Inter"));
        context.TextBaseline = TextBaseline.Top;
        context.TextAlign = TextAlign.Left;
        context.FillText(BackendLabel, (Frame.X + 6, Frame.Y + 6));
    }
}
