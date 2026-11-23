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
    public void SymbolPlacementUsesOneResourceOpcodeInsteadOfExpandingInstructions()
    {
        using var stream = new WireBuffer();
        var writer = new AndroidCommandWriter(stream, new Resources());
        IContext canvas = writer;
        object key = new();
        using (var symbol = canvas.Symbol(new Xui.Core.Canvas.Symbols.SymbolOptions { Key=key, Bounds=(0,0,20,20) }))
        {
            symbol.Context.SetFill(Colors.Red);
            symbol.Context.FillRect((0,0,20,20));
        }
        Assert.Equal(0, stream.Length);
        for (int i=0; i<100; i++) canvas.UseSymbol(key);
        Assert.Equal(800, stream.Length);
        for (int i=0; i<100; i++)
        {
            Assert.Equal((int)DrawOp.Symbol, Int(stream.Data, i*8));
            Assert.Equal(1, Int(stream.Data, i*8+4));
        }
    }

    private static int Int(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
    private static float Float(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadSingleLittleEndian(bytes[offset..]);
    private static int NodeOffset(ReadOnlySpan<byte> packet, int index)
    {
        Assert.Equal(AndroidCompositionPresenter.Magic, Int(packet,0)); Assert.Equal(1,Int(packet,4));
        int p=16+4*Int(packet,8);
        for(int i=0;i<index;i++) p+=136+Math.Max(0,Int(packet,p+132));
        return p;
    }
    [Fact]
    public void MovingCachedNodeKeepsIdentityAndOmitsDrawing()
    {
        using var presenter=new AndroidCompositionPresenter(0,new Resources());
        using var ctx=new RetainedCanvasContext(new Measure(),graphicsNodeFactory:presenter.CreateNode);
        object key=new();
        Frame(ctx,key,true,0);var first=presenter.Encode(ctx.Root).ToArray();
        Assert.Equal(2,Float(first,NodeOffset(first,0)+52)); // Root must receive the physical pixel scale too.
        ctx.AcceptFrame();
        Frame(ctx,key,false,25);var second=presenter.Encode(ctx.Root);int a=NodeOffset(first,1),b=NodeOffset(second,1);
        Assert.Equal(Int(first,a),Int(second,b));Assert.True(Int(first,a+132)>0);Assert.Equal(-1,Int(second,b+132));
        // Origin compensation: translation 25 + child bounds origin 10.
        Assert.Equal(35,Float(second,b+44));Assert.Equal(20,Float(second,b+48));
        Assert.Equal(2,Int(second,12));
    }
    [Fact]
    public void CulledCachedNodesReturnWithSameIdAndNoGraphicsUpload()
    {
        using var presenter=new AndroidCompositionPresenter(0,new Resources());
        using var ctx=new RetainedCanvasContext(new Measure(),graphicsNodeFactory:presenter.CreateNode);
        object key=new();
        void Declare(bool updates)
        {
            using var group=ctx.Group(new GroupOptions {Key=key,Bounds=(0,0,100,100),ViewBacking=true,CacheWhenHidden=true,HasUpdates=updates});
            if(group.NeedsRedraw){ctx.SetFill(Colors.Blue);ctx.FillRect((0,0,100,100));}
        }
        ctx.BeginFrame((0,0,300,500),1);Declare(true);ctx.EndFrame();var first=presenter.Encode(ctx.Root).ToArray();ctx.AcceptFrame();
        ctx.BeginFrame((0,0,300,500),1);ctx.EndFrame();var hidden=presenter.Encode(ctx.Root);Assert.Equal(1,Int(hidden,12));ctx.AcceptFrame();
        ctx.BeginFrame((0,0,300,500),1);Declare(false);ctx.EndFrame();var last=presenter.Encode(ctx.Root);int p=NodeOffset(last,1);
        Assert.Equal(Int(first,NodeOffset(first,1)),Int(last,p));Assert.Equal(-1,Int(last,p+132));
    }
    [Fact]
    public void WireFormatUsesFloat32AndUtf16IndependentOfNativeStructLayout()
    {
        using var buffer=new WireBuffer();var draw=new AndroidCommandWriter(buffer,new Resources());
        draw.FillRect(new Rect(1.25f,2.5f,10,20));draw.FillText("A😀",(5,6));
        Assert.Equal((int)DrawOp.FillRect,Int(buffer.Data,0));Assert.Equal(1.25f,Float(buffer.Data,4));
        Assert.Equal((int)DrawOp.Text,Int(buffer.Data,20));Assert.Equal(3,Int(buffer.Data,32));Assert.Equal(42,buffer.Length);
        Assert.Equal("A😀",System.Text.Encoding.Unicode.GetString(buffer.Data[36..]));
    }
    [Fact]
    public void JavaAndManagedOpcodeTablesMatch()
    {
        var directory=new DirectoryInfo(AppContext.BaseDirectory);
        while(directory!=null&&!File.Exists(Path.Combine(directory.FullName,"Xui.sln")))directory=directory.Parent;
        Assert.NotNull(directory);
        var java=File.ReadAllText(Path.Combine(directory.FullName,"Xui/Runtime/Android/Java/xui/runtime/DrawOp.java"));
        var matches=Regex.Matches(java,@"static final int (\w+) = (\d+);");
        Assert.Equal(Enum.GetValues<DrawOp>().Length,matches.Count);
        foreach(Match match in matches)Assert.Equal(int.Parse(match.Groups[2].Value),(int)Enum.Parse<DrawOp>(match.Groups[1].Value));
    }
    [Fact]
    public void AssetUrisMatchMobilePackageLogicalNames()
    {
        Assert.Equal("Assets/test-512.png", Xui.Runtime.Android.Actual.AndroidResources.ResolveAsset("asset://test-512.png"));
        Assert.Equal("Assets/icons/logo.png", Xui.Runtime.Android.Actual.AndroidResources.ResolveAsset("asset://icons/logo.png"));
        Assert.Throws<NotSupportedException>(() => Xui.Runtime.Android.Actual.AndroidResources.ResolveAsset("asset://../secret"));
    }
    private sealed class GpuFactory : IGpuSurfaceFactory
    {
        public bool IsSupported => true;
        public ICompositionGpuSurface CreateSurface() => new GpuSurface();
        private sealed class GpuSurface : ICompositionGpuSurface
        {
            public bool HasContent => true;
            public void RequestRender(IGpuSurfaceContent content) { }
            public void Dispose() { }
        }
    }
    private sealed class GpuContent : IGpuSurfaceContent { public void Render(IGpuRenderTarget target) => throw new InvalidOperationException("Encoding must not render GPU content."); }
    [Fact]
    public void NativeGpuLeafHasIdentityAndNoCanvasOrPixelPayload()
    {
        using var presenter=new AndroidCompositionPresenter(0,new Resources());
        presenter.CreateNode().Dispose();presenter.CreateNode().Dispose(); // Fixtures reserve the IDs used by the 2D scene.
        using var ctx=new RetainedCanvasContext(new Measure(),new GpuFactory(),presenter.CreateNode,presenter.CreateGpuNode);
        var key=new GpuContent();
        void Record(int x,bool updates)
        {
            ctx.BeginFrame((0,0,300,500),2);
            Assert.True(ctx.GpuSurface(new GroupOptions {Key=key,Bounds=(10,20,100,100),HasUpdates=updates,Transform=AffineTransform.Translate((x,0))}));
            ctx.EndFrame();
        }
        Record(0,true);var first=presenter.Encode(ctx.Root).ToArray();ctx.AcceptFrame();
        Record(30,false);var second=presenter.Encode(ctx.Root);int a=NodeOffset(first,1),b=NodeOffset(second,1);
        Assert.Equal(Int(first,a),Int(second,b));Assert.Equal(-2,Int(second,b+132));
        Assert.Equal(40,Float(second,b+44));Assert.Equal(b+136,second.Length);
        var directory=Path.Combine(AppContext.BaseDirectory,"AndroidFixtures");Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory,"gpu.bin"),first);
        File.WriteAllBytes(Path.Combine(directory,"gpu-moved.bin"),second.ToArray());
    }
    [Fact]
    public void ShaderAssetIdentityUsesExactGeneratedSourceAndStage()
    {
        var backend=new Xui.GPU.Backends.Hlsl.HlslCodeGenerator();
        var source=Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest.CubeShader.GenerateShaderSource(backend);
        var path=VulkanDevice.ShaderAsset(source,"VSMain");
        Assert.StartsWith("xui-shaders/",path);Assert.EndsWith(".VSMain.spv",path);
        Assert.NotEqual(path,VulkanDevice.ShaderAsset(source+"\n","VSMain"));
        Assert.NotEqual(path,VulkanDevice.ShaderAsset(source,"PSMain"));
    }
    [Fact]
    public void ExportNativeReplayFixtures()
    {
        // Consumed by Native/run.sh on an actual Android runtime, including hardware Views.
        var directory=Path.Combine(AppContext.BaseDirectory,"AndroidFixtures");Directory.CreateDirectory(directory);
        using var presenter=new AndroidCompositionPresenter(0,new Resources());
        using var ctx=new RetainedCanvasContext(new Measure(),graphicsNodeFactory:presenter.CreateNode);
        object key=new();
        Frame(ctx,key,true,0);File.WriteAllBytes(Path.Combine(directory,"initial.bin"),presenter.Encode(ctx.Root).ToArray());ctx.AcceptFrame();
        Frame(ctx,key,false,25);File.WriteAllBytes(Path.Combine(directory,"moved.bin"),presenter.Encode(ctx.Root).ToArray());ctx.AcceptFrame();
        foreach(var shadow in new[]{false,true})
        {
            ctx.BeginFrame((0,0,300,500),1);
            ctx.SetFill(Colors.White);ctx.FillRect((0,0,300,500));
            using(var card=ctx.Group(new GroupOptions {Key=key,Bounds=(50,60,180,120),ViewBacking=true,
                Background=Colors.White,CornerRadius=16,
                Shadow=shadow?new OutlineShadow(new Color(0,0,0,(System.Runtime.InteropServices.NFloat).06),6,(0,2)):null})) { }
            ctx.EndFrame();File.WriteAllBytes(Path.Combine(directory,shadow?"shadow.bin":"shadow-none.bin"),presenter.Encode(ctx.Root).ToArray());ctx.AcceptFrame();
        }
        using var bytes=new WireBuffer();var draw=new AndroidCommandWriter(bytes,new Resources());
        draw.SetFill(Colors.Red);draw.FillRect((0,0,10,10));draw.GlobalAlpha=(System.Runtime.InteropServices.NFloat).5;
        draw.DrawImage(null!,new Rect(20,0,10,10),(System.Runtime.InteropServices.NFloat).5);
        draw.Save();draw.SetFill(Colors.Blue);draw.GlobalAlpha=1;draw.FillRect((40,0,10,10));draw.Restore();
        draw.FillRect((60,0,10,10));
        draw.SetStroke(Colors.White);draw.LineWidth=2;draw.SetLineDash([2,4]);draw.Save();draw.SetLineDash([1,2,3]);draw.Restore();draw.LineDashOffset=1;
        draw.BeginPath();draw.MoveTo((0,30));draw.LineTo((80,30));draw.Stroke();
        File.WriteAllBytes(Path.Combine(directory,"drawing.bin"),bytes.Data.ToArray());
        Assert.True(bytes.Length>0);
    }
}
