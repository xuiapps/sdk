using Xui.Core.Canvas;
using Xui.Core.Canvas.Symbols;
using Xui.Core.Math2D;
using Xui.Runtime.IOS.Actual;

namespace Xui.Tests.Component.IOS;

public sealed partial class IOSDrawingContextTest
{
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
