using Xui.Core.Canvas;

namespace Xui.Apps.TestApp.Pages.Floorplan;

/// <summary>Vector map with retained room and detail-level composition groups.</summary>
public sealed class FloorplanView : FloorplanCameraView
{
    public FloorplanDrawing Drawing { get; }
    private readonly FloorplanDrawing.Composition composition;
    public FloorplanView() : this(FloorplanDrawing.Shared) { }
    public FloorplanView(FloorplanDrawing drawing)
    { Drawing = drawing; composition = drawing.CreateComposition(); }

    protected override void RenderCore(IContext context)
    {
        if (!HasCamera) return;
        composition.Render(context, Frame, CameraTransform, DetailLevel, VisibleWorld, MarkerOpacity,
            DetailLevel >= 2, UseSymbols, DetailBackingScale);
    }
}
