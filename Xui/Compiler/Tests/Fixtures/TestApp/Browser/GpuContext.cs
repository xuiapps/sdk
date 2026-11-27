using System;
using System.Collections.Generic;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;

namespace TestAppCanvas;

public sealed partial class CanvasContext
{
    public BrowserGpuDevice GpuDevice { get; set; }
    private sealed class Surface(object key, BrowserGpuTarget target)
    { public readonly object Key=key;public BrowserGpuTarget Target=target;public int Seen; }
    private readonly List<Surface> gpuSurfaces = new();
    private int gpuFrame;
    public void BeginGpuFrame() => gpuFrame++;
    public void EndGpuFrame()
    {
        for(int i=gpuSurfaces.Count-1;i>=0;i--)
            if(gpuSurfaces[i].Seen!=gpuFrame) {gpuSurfaces[i].Target.Dispose();gpuSurfaces.RemoveAt(i);}
    }
    partial void DisposeGpu() {foreach(var surface in gpuSurfaces)surface.Target.Dispose();gpuSurfaces.Clear();}
    public bool GpuSurface(scoped in GroupOptions options)
    {
        if(GpuDevice is null)return false;
        var content=options.Key as IGpuSurfaceContent ?? throw new ArgumentException("GPU surface key must render GPU content.");
        var bounds=options.Bounds ?? throw new ArgumentException("GPU surface requires bounds.");
        if(bounds.Width<=0 || bounds.Height<=0)return true;
        double scale=NativeGpu.Invoke("Scale",0)/1024d;
        int width=Math.Max(1,(int)Math.Ceiling((double)bounds.Width*scale)),height=Math.Max(1,(int)Math.Ceiling((double)bounds.Height*scale));
        Surface surface=null;
        for(int i=0;i<gpuSurfaces.Count;i++) {var candidate=gpuSurfaces[i];if(ReferenceEquals(candidate.Key,options.Key))surface=candidate;}
        bool changed=surface is null || surface.Target.Width!=width || surface.Target.Height!=height;
        if(surface is null){surface=new Surface(options.Key,new BrowserGpuTarget(GpuDevice,width,height));gpuSurfaces.Add(surface);}
        else if(changed){surface.Target.Dispose();surface.Target=new BrowserGpuTarget(GpuDevice,width,height);}
        surface.Seen=gpuFrame;
        if(changed || options.HasUpdates) content.Render(surface.Target);
        using var group=Group(options with {ViewBacking=true});
        NativeCanvas.Command("GpuTarget",surface.Target.Handle);
        NativeCanvas.Command("GpuDraw",(double)bounds.X,(double)bounds.Y,(double)bounds.Width,(double)bounds.Height);
        return true;
    }
}
