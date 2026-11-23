using System.Buffers.Binary;
using System.Text.RegularExpressions;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Runtime.Android.Composition;
using Xunit;
using Xui.Core.Canvas.Composition.GPU;
using Xui.GPU.Hardware;
using Xui.Runtime.Android.GPU;

namespace Xui.Tests.Component.Android;

public partial class AndroidCompositionTest
{
    private sealed class Resources : IAndroidResources
    {
        public int Symbol(Xui.Core.Canvas.Symbols.SymbolContent content) => 1;
        public int Font(Xui.Core.Canvas.Font font) => 1;
        public int Image(IImage image) => 1;
    }

    private sealed class Measure : ITextDrawingContext
    {
        public TextAlign TextAlign { set { } }
        public TextBaseline TextBaseline { set { } }
        public void SetFont(Xui.Core.Canvas.Font font) { }
        public TextMetrics MeasureText(string text) => default;
        public void FillText(string text, Point position) => throw new NotSupportedException();
    }

    private static void Frame(RetainedCanvasContext ctx, object key, bool update, int x)
    {
        ctx.BeginFrame((0,0,300,500),2);
        using(var group=ctx.Group(new GroupOptions { Key=key, Bounds=(10,20,100,150), ViewBacking=true,
            Transform=AffineTransform.Translate((x,0)), HasUpdates=update }))
        {
            if(group.NeedsRedraw) {ctx.SetFill(Colors.Red);ctx.FillRect((10,20,100,150));}
        }
        ctx.EndFrame();
    }
}
