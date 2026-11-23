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
    [Fact]
    public void WarmTransformFramesAllocateNoManagedObjects()
    {
        using var presenter=new AndroidCompositionPresenter(0,new Resources());
        using var ctx=new RetainedCanvasContext(new Measure(),graphicsNodeFactory:presenter.CreateNode);
        object key=new();
        for(int i=0;i<20;i++){Frame(ctx,key,i==0,i);presenter.Encode(ctx.Root);ctx.AcceptFrame();}
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100;i++){Frame(ctx,key,false,i);presenter.Encode(ctx.Root);ctx.AcceptFrame();}
        Assert.Equal(0,GC.GetAllocatedBytesForCurrentThread()-before);
    }
}
