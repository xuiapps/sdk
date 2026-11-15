using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Replays a retained display list directly into UIKit's drawRect context.</summary>
internal sealed class IOSCompositionDrawingView : UIView
{
    public static new readonly Class Class = UIView.Class
        .Extend("XUIIOSCompositionDrawingView")
        .AddMethod("drawRect:", DrawRect)
        .Register();

    private static readonly Sel Interaction = new("setUserInteractionEnabled:"),
        Opaque = new("setOpaque:"), Hidden = new("setHidden:"),
        ContentScale = new("setContentScaleFactor:"), Layer = new("layer"),
        DrawsAsynchronously = new("setDrawsAsynchronously:");

    private readonly GraphicsNode node;
    private readonly IOSDrawingContext drawing;
    private NFloat scale;
    private bool hidden;

    public IOSCompositionDrawingView(GraphicsNode node, IOSDrawingContext drawing) : base(Class.New())
    {
        this.node = node;
        this.drawing = drawing;
        ObjC.objc_msgSend(this, Interaction, false);
        ObjC.objc_msgSend(this, Opaque, false);
        // The display lists and shared drawing context are owned by the UI thread.
        ObjC.objc_msgSend(objc_msgSend_retIntPtr(this, Layer), DrawsAsynchronously, false);
        ClearsContextBeforeDrawing = true;
    }

    public void Update(NFloat scale)
    {
        var scaleChanged = this.scale != scale;
        if (scaleChanged)
        {
            this.scale = scale;
            ObjC.objc_msgSend(this, ContentScale, scale);
        }
        var empty = !node.HasDrawing || node.RasterBounds.Width <= 0 || node.RasterBounds.Height <= 0;
        if (hidden != empty)
        {
            hidden = empty;
            ObjC.objc_msgSend(this, Hidden, empty);
        }
        if (!empty && (node.Updating || scaleChanged)) SetNeedsDisplay();
    }

    private static void DrawRect(nint self, nint selector, CGRect rect) =>
        Marshalling.Get<IOSCompositionDrawingView>(self).Replay();

    private void Replay()
    {
        if (!node.HasDrawing) return;
        var cg = UIGraphicsGetCurrentContext();
        if (cg == 0) return;
        // UIKit supplies a local drawing context; recorded commands use the group's coordinates.
        CGContextRef.CGContextSaveGState(cg);
        try
        {
            CGContextRef.CGContextTranslateCTM(cg, -node.RasterBounds.X, -node.RasterBounds.Y);
            var context = drawing.Bind();
            ((ITransformContext)context).SetTransform(AffineTransform.Identity);
            node.Inherited.Replay(context);
            node.Commands.Replay(context);
        }
        finally
        {
            drawing.Unbind();
            CGContextRef.CGContextRestoreGState(cg);
        }
    }
}
