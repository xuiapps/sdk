using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Runtime.IOS.Actual;
using Xui.Runtime.Software.Actual;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Tests.Component.IOS;

public sealed partial class IOSDrawingContextTest
{
    [CoreGraphicsFact]
    public void FloorplanRoomCompositionPreservesPainterOrderAndDoesNotClipGeometry()
    {
        var drawing = FloorplanDrawing.Shared;
        using var measure = new SvgDrawingContext((1000, 1000), Stream.Null, Xui.Core.Fonts.Inter.Source);
        for (var level = 0; level < 4; level++)
        foreach (var zoomed in new[] { false, true })
        {
            var visible = zoomed ? new Rect(300, 400, (nfloat)1000 / 3, (nfloat)1000 / 3) : new Rect(0, 120, 1000, 1000);
            nfloat scale = zoomed ? 3 : 1;
            var camera = AffineTransform.Scale((scale, scale)) * AffineTransform.Translate((-visible.X, -visible.Y));
            // This macOS-hosted test exercises CoreGraphics geometry. iOS text uses UIKit;
            // labels and marker text are reviewed in the complete SVG reference scenarios.
            using var recorder = new RetainedCanvasContext(measure);
            recorder.BeginFrame((0, 0, 1000, 1000), 1);
            drawing.CreateComposition().Render(recorder, (0, 0, 1000, 1000), camera, level, visible,
                showLabels: false, useSymbols: false, rasterScale: 4);
            recorder.EndFrame();
            using var actual = new Bitmap(1000);
            using var expected = new Bitmap(1000);
            using var retained = new IOSDrawingContext(Xui.Core.Fonts.Inter.Source);
            using var direct = new IOSDrawingContext(Xui.Core.Fonts.Inter.Source);
            Replay(recorder.Root);
            direct.Bind(expected.Handle);
            ((IContext)direct).Transform(camera);
            drawing.Render(direct, level, visible, showLabels: false, useSymbols: false);
            direct.Unbind();
            var a = actual.Pixels();
            var b = expected.Pixels();
            double error = 0, coverageA = 0, coverageB = 0;
            for (var i = 0; i < a.Length; i++) error += Math.Abs(a[i] - b[i]);
            for (var i = 3; i < a.Length; i += 4) { coverageA += a[i]; coverageB += b[i]; }
            Assert.True(error / a.Length < .1, $"LoD {level}, zoomed={zoomed}: mean channel error {error / a.Length}");
            Assert.InRange(coverageA / coverageB, .999, 1.001);

            void Replay(Node node)
            {
                var cg = actual.Handle;
                CGContextRef.CGContextSaveGState(cg);
                var m = node.Placement;
                CGContextRef.CGContextConcatCTM(cg, new CGAffineTransform(m.A, m.B, m.C, m.D, m.Tx, m.Ty));
                var bounds = node.RasterBounds;
                var rect = new CGRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                if (node.Options.ClipToBounds) CGContextRef.CGContextClipToRect(cg, rect);
                if (node is GraphicsNode { HasDrawing: true } graphics)
                {
                    // UIKit clips a drawing surface to its backing bounds even when the
                    // container permits its composition children to extend outside them.
                    CGContextRef.CGContextSaveGState(cg);
                    CGContextRef.CGContextClipToRect(cg, rect);
                    retained.Bind(cg);
                    graphics.Inherited.Replay(retained);
                    graphics.Commands.Replay(retained);
                    retained.Unbind();
                    CGContextRef.CGContextRestoreGState(cg);
                }
                for (var i = 0; i < node.Count; i++) Replay(node[i]);
                CGContextRef.CGContextRestoreGState(cg);
            }
        }
    }
}
