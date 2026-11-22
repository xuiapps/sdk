using Xui.Core.Canvas;
using Xui.Core.Canvas.Symbols;
using Xui.Core.Math2D;
using Xui.Runtime.IOS.Actual;

namespace Xui.Tests.Component.IOS;

public sealed partial class IOSDrawingContextTest
{
    [CoreGraphicsFact]
    public void PdfSymbolsPreserveBoundsClippingRotationAndReflections()
    {
        using var actual = new Bitmap(192);
        using var expected = new Bitmap(192);
        using IContext cached = new IOSDrawingContext();
        using IContext direct = new IOSDrawingContext();
        ((IOSDrawingContext)cached).Bind(actual.Handle);
        ((IOSDrawingContext)direct).Bind(expected.Handle);
        static void Draw(IContext c)
        {
            c.SetFill(Colors.Red); c.FillRect((-12, 4, 20, 8));
            c.SetFill(Colors.Blue); c.FillRect((-12, 12, 7, 8));
        }
        object key = new();
        using (var definition = cached.Symbol(new SymbolOptions { Key=key, Bounds=(-12,4,20,16) }))
            Draw(definition.Context);
        void Place(IContext c, bool symbol)
        {
            c.SetFill(Colors.Green);
            // The initial compile happens under a partial placement clip. That clip
            // must not become permanent content of the shared definition.
            c.Save(); c.BeginPath(); c.Rect((24,20,8,20)); c.Clip();
            c.Translate((36,16));
            if (symbol) c.UseSymbol(key); else Draw(c);
            c.Restore();
            c.Save(); c.Translate((100,100)); c.Rotate((nfloat).35); c.Scale((3,-2));
            if (symbol) c.UseSymbol(key); else Draw(c);
            c.Restore();
            // Verify placement restores the enclosing paint state as well.
            c.FillRect((150,150,10,10));
        }
        Place(cached,true); Place(direct,false);
        var a=actual.Pixels(); var b=expected.Pixels();
        double error=0, coverageA=0, coverageB=0;
        for (int i=0; i<a.Length; i++) error+=Math.Abs(a[i]-b[i]);
        for (int i=3; i<a.Length; i+=4) { coverageA+=a[i]; coverageB+=b[i]; }
        Assert.InRange(error/a.Length,0,2);
        Assert.True(coverageB>10000);
        Assert.InRange(coverageA/coverageB,.98,1.02);
        ((IOSDrawingContext)cached).Unbind(); ((IOSDrawingContext)direct).Unbind();
    }

    [CoreGraphicsFact]
    public void FloorplanSymbolsMatchDirectDrawingAtEveryDetailLevel()
    {
        var drawing = Xui.Apps.TestApp.Pages.Floorplan.FloorplanDrawing.Shared;
        for (int level=0; level<4; level++)
        {
            using var actual = new Bitmap(1000);
            using var expected = new Bitmap(1000);
            using IContext cached = new IOSDrawingContext(Xui.Core.Fonts.Inter.Source);
            using IContext direct = new IOSDrawingContext(Xui.Core.Fonts.Inter.Source);
            ((IOSDrawingContext)cached).Bind(actual.Handle);
            ((IOSDrawingContext)direct).Bind(expected.Handle);
            cached.Translate((0,-120)); direct.Translate((0,-120));
            drawing.Render(cached,level,(0,120,1000,760),showLabels:false,useSymbols:true);
            drawing.Render(direct,level,(0,120,1000,760),showLabels:false,useSymbols:false);
            var a=actual.Pixels(); var b=expected.Pixels();
            double error=0;
            for(int i=0;i<a.Length;i++) error+=Math.Abs(a[i]-b[i]);
            Assert.True(error/a.Length < 1, $"LoD {level}: mean channel error {error/a.Length}");
            ((IOSDrawingContext)cached).Unbind(); ((IOSDrawingContext)direct).Unbind();
        }
    }

    [CoreGraphicsFact]
    public void NativePdfSymbolMatchesDirectCurvesAtDifferentScalesAndReusesWithoutAllocating()
    {
        using var actual = new Bitmap(128);
        using var expected = new Bitmap(128);
        using IContext context = new IOSDrawingContext();
        using IContext direct = new IOSDrawingContext();
        ((IOSDrawingContext)context).Bind(actual.Handle);
        ((IOSDrawingContext)direct).Bind(expected.Handle);
        object key=new();
        static void Draw(IContext c)
        {
            c.SetFill(new LinearGradient((0,0),(20,20),[new(0,Colors.Red),new(1,Colors.Blue)]));
            c.BeginPath(); c.RoundRect((2,2,16,12),3); c.Fill();
            c.SetStroke(Colors.White); c.LineWidth=1;
            c.BeginPath(); c.Arc((10,10),6,0,(nfloat)4.5); c.Stroke();
        }
        using (var symbol=context.Symbol(new SymbolOptions { Key=key, Bounds=(0,0,20,20) }))
            Draw(symbol.Context);
        Assert.All(actual.Pixels(), b=>Assert.Equal(0,b));
        foreach (nfloat scale in new nfloat[] {1, 2, 4})
        {
            context.Save(); direct.Save();
            context.Scale((scale,scale)); direct.Scale((scale,scale));
            context.UseSymbol(key); Draw(direct);
            context.Restore(); direct.Restore();
        }
        var a=actual.Pixels(); var b=expected.Pixels();
        double error=0;
        for(int i=0;i<a.Length;i++) error+=Math.Abs(a[i]-b[i]);
        Assert.InRange(error/a.Length,0,2);
        for (int i=0;i<20;i++) context.UseSymbol(key);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for (int i=0;i<100;i++) context.UseSymbol(key);
        Assert.Equal(0,GC.GetAllocatedBytesForCurrentThread()-before);
        before=GC.GetAllocatedBytesForCurrentThread();
        for (int i=0; i<100; i++)
        {
            context.Save(); context.Scale((1+(nfloat)i/50, 1+(nfloat)i/50));
            context.UseSymbol(key); context.Restore();
        }
        Assert.Equal(0,GC.GetAllocatedBytesForCurrentThread()-before);
        ((IOSDrawingContext)context).Unbind(); ((IOSDrawingContext)direct).Unbind();
    }
}
