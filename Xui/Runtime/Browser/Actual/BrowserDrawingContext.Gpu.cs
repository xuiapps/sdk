using System;
using System.Collections.Generic;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.GPU;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;

namespace Xui.Runtime.Browser.Actual;

public partial class BrowserDrawingContext
{
    internal BrowserGpuDevice? GpuDevice { get; set; }
    private readonly Dictionary<object, Surface> surfaces = new(ReferenceEqualityComparer.Instance);
    private readonly List<object> omittedSurfaces = new();
    private int gpuFrame;
    private sealed class Surface(BrowserGpuTarget target)
    {
        internal BrowserGpuTarget Target = target;
        internal int Seen;
        internal bool HasContent;
    }

    public bool GpuSurface(scoped in GroupOptions options) => GpuSurface(in options, out _);

    public bool GpuSurface(scoped in GroupOptions options, out GpuSurfaceFailure failure)
    {
        ICompositionContext.ValidateGpuSurface(in options);
        failure = GpuSurfaceFailure.None;
        if (GpuDevice is null) { failure = GpuSurfaceFailure.NotSupported; return false; }
        if (options.Key is not IGpuSurfaceContent content)
            throw new ArgumentException("GPU surface key must implement IGpuSurfaceContent.", nameof(options));
        var bounds = options.Bounds ?? throw new ArgumentException("GPU surface requires bounds.", nameof(options));
        if (bounds.Width <= 0 || bounds.Height <= 0) return true;
        var matrix = transform * options.Transform;
        var scaleX = Math.Sqrt((double)(matrix.A * matrix.A + matrix.B * matrix.B)) * pixelRatio;
        var scaleY = Math.Sqrt((double)(matrix.C * matrix.C + matrix.D * matrix.D)) * pixelRatio;
        int width = Math.Max(1, checked((int)Math.Ceiling((double)bounds.Width * scaleX)));
        int height = Math.Max(1, checked((int)Math.Ceiling((double)bounds.Height * scaleY)));
        if (!surfaces.TryGetValue(options.Key, out var surface))
        {
            surface = new((BrowserGpuTarget)GpuDevice.CreateRenderTarget(width, height));
            surfaces.Add(options.Key, surface);
        }
        else if (surface.Target.Width != width || surface.Target.Height != height)
        {
            surface.Target.Dispose();
            surface.Target = (BrowserGpuTarget)GpuDevice.CreateRenderTarget(width, height);
            surface.HasContent = false;
        }
        surface.Seen = gpuFrame;
        if (!surface.HasContent || options.HasUpdates)
        {
            content.Render(surface.Target);
            surface.HasContent = true;
        }
        using var group = Group(options with { ViewBacking = true });
        Write(Op.GpuSurface, surface.Target.Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        return true;
    }

    internal void EndFrame()
    {
        // Replay references targets, so release omitted resources only after the batch is consumed.
        omittedSurfaces.Clear();
        foreach (var pair in surfaces)
            if (pair.Value.Seen != gpuFrame) omittedSurfaces.Add(pair.Key);
        foreach (var key in omittedSurfaces)
        {
            surfaces[key].Target.Dispose();
            surfaces.Remove(key);
        }
    }
    private void DisposeGpuSurfaces()
    {
        foreach (var surface in surfaces.Values) surface.Target.Dispose();
        surfaces.Clear();
        omittedSurfaces.Clear();
    }
}
