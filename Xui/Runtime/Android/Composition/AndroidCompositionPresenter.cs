using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.GPU;
using Xui.Runtime.Android.GPU;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Runtime.Android.Actual;
using Xui.Runtime.Android.Interop;

namespace Xui.Runtime.Android.Composition;

internal sealed class AndroidCompositionPresenter : IDisposable, IGpuSurfaceFactory
{
    internal const int Magic = 0x58554931; // XUI1
    private readonly nint host;
    private readonly WireBuffer frame = new();
    private readonly AndroidCommandWriter drawing;
    private readonly List<int> removed = new();
    private readonly Action<int> release;
    private int nextId;
    private readonly VulkanDevice? device;
    private readonly Action? invalidate;
    private readonly Dictionary<int,AndroidGpuSurface> gpu = new();
    public bool IsSupported => device?.IsSupported == true;
    public ICompositionGpuSurface CreateSurface() => new AndroidGpuSurface(device!,invalidate!);
    internal GpuNode CreateGpuNode() => new AndroidGpuNode(++nextId,release);
    internal void ResumeGpu() {foreach(var surface in gpu.Values)surface.Resume();}
    internal void BindGpu(int id,nint window,int width,int height) {if(gpu.TryGetValue(id,out var surface))surface.Bind(window,width,height);}
    internal AndroidCompositionPresenter(nint host, IAndroidResources resources,VulkanDevice? device=null,Action? invalidate=null)
    { this.host=host;this.device=device;this.invalidate=invalidate;drawing=new(frame,resources);release=id=>{removed.Add(id);gpu.Remove(id);}; }
    internal GraphicsNode CreateNode() => new AndroidGraphicsNode(++nextId, release);

    internal void Present(RetainedCanvasContext context)
    {
        TrackGpu(context.Root);
        Encode(context.Root);
        AndroidBridge.Invoke(host, 1, frame);
        removed.Clear();
        PresentGpu(context.Root);
        context.AcceptFrame();
    }
    internal ReadOnlySpan<byte> Encode(GraphicsNode root)
    {
        frame.Clear(); frame.Int(Magic); frame.Int(1);
        frame.Int(removed.Count); foreach (int id in removed) frame.Int(id);
        int countOffset = frame.Length; frame.Int(0);
        int count = Visit((AndroidGraphicsNode)root, 0, 0, default);
        frame.PatchInt(countOffset, count);
        return frame.Data;
    }
    private void TrackGpu(Node node)
    {
        if(node is AndroidGpuNode gpuNode)gpu[gpuNode.Id]=(AndroidGpuSurface)gpuNode.Surface!;
        for(int i=0;i<node.Count;i++)TrackGpu(node[i]);
    }
    private static void PresentGpu(Node node)
    {
        if(node is GpuNode { Surface: AndroidGpuSurface surface })surface.Present();
        for(int i=0;i<node.Count;i++)PresentGpu(node[i]);
    }
    private int Visit(Node node, int parent, int index, Point parentOrigin)
    {
        var b = node.RasterBounds; var o = node.Options; var m = node.Placement;
        // A native view's local (0,0) corresponds to RasterBounds.TopLeft.
        // Compensate both child and parent origins when mapping native geometry.
        m = AffineTransform.Translate((-parentOrigin.X, -parentOrigin.Y)) * m * AffineTransform.Translate((b.X, b.Y));
        int id=node switch {AndroidGraphicsNode g=>g.Id,AndroidGpuNode g=>g.Id,_=>throw new NotSupportedException("Unknown Android node")};
        frame.Int(id); frame.Int(parent); frame.Int(index);
        frame.Rect(b); frame.Matrix(m); frame.Float(node.Scale);
        frame.Int(o.ClipToBounds ? 1 : 0); frame.Float(o.CornerRadius);
        frame.Color(o.Background ?? default); frame.Int((int)o.Backdrop);
        frame.Color(o.Border?.Color ?? default); frame.Float(o.Border?.Width ?? 0);
        frame.Color(o.Shadow?.Color ?? default); frame.Float(o.Shadow?.Radius ?? 0);
        frame.Vector(o.Shadow?.Offset ?? default);
        int sizeOffset = frame.Length; frame.Int(-1);
        if(node is AndroidGpuNode)
        {
            frame.PatchInt(sizeOffset,-2); // Native Vulkan surface; no Canvas command stream.
        }
        else if (node is GraphicsNode graphics && node.Updating)
        {
            int start = frame.Length;
            graphics.Inherited.Replay(drawing); graphics.Commands.Replay(drawing);
            frame.PatchInt(sizeOffset, frame.Length - start);
        }
        int count = 1;
        for (int i = 0; i < node.Count; i++)
        {
            count += Visit(node[i], id, i, b.TopLeft);
        }
        return count;
    }
    public void Dispose() { frame.Dispose(); removed.Clear();gpu.Clear(); }
}
