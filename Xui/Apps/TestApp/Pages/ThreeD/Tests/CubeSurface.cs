using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.ThreeD.Tests;

/// <summary>Retains the rendered image while navigation composes the moving page.</summary>
public abstract class CubeSurface : View
{
    protected NFloat rotationX = 0.3f;
    protected NFloat rotationY = 0.3f;
    private Point? dragStart;
    private NFloat dragStartRotX, dragStartRotY;
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
            if (!value) dragStart = null;
            if (value && dirty) InvalidateRender();
        }
    }

    protected abstract string BackendLabel { get; }
    protected abstract bool RenderPixels(byte[] pixels);

    /// <summary>Marks scene changes without discarding the image used during a transition.</summary>
    protected void InvalidateScene()
    {
        dirty = true;
        if (IsLive) InvalidateRender();
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (!IsLive) return;
        if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            CapturePointer(e.PointerId, PointerGestures.Drag);
            dragStart = e.State.Position;
            dragStartRotX = rotationX;
            dragStartRotY = rotationY;
        }
        else if (e.Type == PointerEventType.Move && dragStart is { } start)
        {
            var delta = e.State.Position - start;
            rotationY = dragStartRotY + delta.X * 0.01f;
            rotationX = dragStartRotX + delta.Y * 0.01f;
            InvalidateScene();
        }
        else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture)
        {
            dragStart = null;
            if (e.Type != PointerEventType.LostCapture) ReleasePointer(e.PointerId);
        }
        base.OnPointerEvent(ref e, phase);
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
