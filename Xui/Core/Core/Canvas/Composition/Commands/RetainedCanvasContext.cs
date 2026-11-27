using Xui.Core.Canvas.Composition.Commands.Records;
#pragma warning disable CS1591 // Runtime-facing retained scene implementation.
using Xui.Core.Math2D;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;
namespace Xui.Core.Canvas.Composition.Commands;

/// <summary>A keyed drawing tree. Each node draws its background first, then its child surfaces.</summary>
public sealed partial class RetainedCanvasContext : IContext
{
    // Measurement must use the same alignment and baseline as deferred text drawing.
    private readonly ITextDrawingContext measure;
    private readonly IGpuSurfaceFactory? gpuSurfaces;
    public GraphicsNode Root { get; }
    private readonly Func<GraphicsNode> graphicsNodeFactory;
    private readonly Func<GpuNode> gpuNodeFactory;
    private GraphicsNode current;
    private readonly Stack<SavedState> saves = new();
    private readonly Stack<GroupFrame> groups = new();
    private PaintState state;
    private long frame;
    private bool recording;
    private nfloat scale;

    /// <summary>Creates a recorder, optionally using platform-specific nodes for the entire tree.</summary>
    /// <param name="measure">Text service used while recording.</param>
    /// <param name="gpuSurfaces">Optional native GPU surface provider.</param>
    /// <param name="graphicsNodeFactory">Creates fresh graphics nodes for the root and keyed groups.</param>
    /// <param name="gpuNodeFactory">Creates fresh GPU leaves. Cached nodes retain their instance and resources.</param>
    public RetainedCanvasContext(ITextDrawingContext measure, IGpuSurfaceFactory? gpuSurfaces = null,
        Func<GraphicsNode>? graphicsNodeFactory = null, Func<GpuNode>? gpuNodeFactory = null)
    {
        this.measure=measure; this.gpuSurfaces=gpuSurfaces;
        this.graphicsNodeFactory=graphicsNodeFactory ?? (static () => new GraphicsNode());
        this.gpuNodeFactory=gpuNodeFactory ?? (static () => new GpuNode());
        current=Root=this.graphicsNodeFactory();
    }
    public void BeginFrame(Rect bounds, nfloat scale)
    {
        if(recording) throw new InvalidOperationException("A composition frame is already open.");
        this.scale=scale; frame++; recording=true; current=Root; saves.Clear(); groups.Clear();
        state=new PaintState { Matrix=AffineTransform.Identity, Font=new Font() };
        Root.Options=new GroupOptions { Key=Root, Bounds=bounds, ViewBacking=true };
        Root.BeginUpdate(); Root.State.Clear(); Root.Inherited.Clear(); Root.RasterBounds=bounds; Root.Scale=scale;
        SetFont(state.Font);
        TextAlign=TextAlign.Start;
        TextBaseline=TextBaseline.Alphabetic;
    }
    public void EndFrame()
    {
        if(!recording || groups.Count!=0 || saves.Count!=0) throw new InvalidOperationException("Unbalanced composition frame.");
        Root.Prune(frame); recording=false;
    }
    public void AcceptFrame() => Root.Accept();
    public void AbortFrame() { recording=false; saves.Clear(); groups.Clear(); Root.Invalidate(); current=Root; }
    public void Dispose() { Root.Dispose(); saves.Clear(); groups.Clear(); }

    private void Emit<T>(in T command, ReadOnlySpan<GradientStop> gradients=default,
        ReadOnlySpan<char> text=default, ReadOnlySpan<nfloat> numbers=default,
        Font font=default, object? resource=null) where T : unmanaged
    {
        if(!recording) throw new InvalidOperationException("Drawing requires an open frame.");
        if(!current.Updating) throw new InvalidOperationException("The group is cached; check NeedsRedraw before drawing.");
        var op = CommandLayout.Op(in command);
        var draws = CommandLayout.Draws(op);
        if(draws && current.Count!=0)
            throw new InvalidOperationException("Draw background content before child groups. Put foreground drawing in an overlay group.");
        current.Commands.Add(in command, gradients, text, numbers, font, resource);
        if(draws) { current.HasDrawing=true; return; }
        if(op==CanvasOp.Save) saves.Push(new(current,state,current.State.Count));
        if(op==CanvasOp.Restore)
        {
            if(!saves.TryPeek(out var saved) || saved.Node!=current) throw new InvalidOperationException("Unbalanced Save/Restore.");
            saves.Pop(); state=saved.State; current.State.Truncate(saved.Count); RestoreTextState(); return;
        }
        current.State.Add(in command, gradients, text, numbers, font, resource);
        switch(op)
        {
            case CanvasOp.Font: state.Font=font; measure.SetFont(font); break;
            case CanvasOp.TextAlign: state.TextAlign=CommandLayout.Read<TextAlignCommand,T>(in command).Value; measure.TextAlign=state.TextAlign; break;
            case CanvasOp.TextBaseline: state.TextBaseline=CommandLayout.Read<TextBaselineCommand,T>(in command).Value; measure.TextBaseline=state.TextBaseline; break;
            case CanvasOp.Clip: state.Clipped=true; break;
            case CanvasOp.Translate: state.Matrix=state.Matrix*AffineTransform.Translate(CommandLayout.Read<TranslateCommand,T>(in command).Value); break;
            case CanvasOp.Scale: state.Matrix=state.Matrix*AffineTransform.Scale(CommandLayout.Read<ScaleCommand,T>(in command).Value); break;
            case CanvasOp.Rotate: state.Matrix=state.Matrix*AffineTransform.Rotate(CommandLayout.Read<RotateCommand,T>(in command).Angle); break;
            case CanvasOp.Transform: state.Matrix=state.Matrix*CommandLayout.Read<TransformCommand,T>(in command).Value; break;
            case CanvasOp.SetTransform: state.Matrix=CommandLayout.Read<SetTransformCommand,T>(in command).Value; break;
        }
    }

    public bool GpuSurface(scoped in GroupOptions options) => GpuSurface(in options, out _);

    public bool GpuSurface(scoped in GroupOptions options, out GpuSurfaceFailure failure)
    {
        ICompositionContext.ValidateGpuSurface(in options);
        if (!recording) throw new InvalidOperationException("GPU surfaces require an open frame.");
        failure = GpuSurfaceFailure.None;
        if (state.Clipped) { failure = GpuSurfaceFailure.PortableFallbackRequired; return false; }
        if (gpuSurfaces?.IsSupported != true) { failure = GpuSurfaceFailure.NotSupported; return false; }
        // Acquire before changing the tree, so expected creation failure leaves composition untouched.
        ICompositionGpuSurface? created = null;
        if (!current.ByKey.TryGetValue(options.Key, out var existing) || existing is not GpuNode { Surface: not null })
        {
            try { created = gpuSurfaces.CreateSurface(); }
            catch (NotSupportedException) { failure = GpuSurfaceFailure.NotSupported; return false; }
        }
        GpuNode node;
        try { node = (GpuNode)GetOrCreateNode(options.Key, gpu: true); }
        catch { created?.Dispose(); throw; }
        node.Surface ??= created;
        var redraw = NeedsUpdate(node, in options);
        ConfigureNode(node, options with { ViewBacking = true });
        node.Updating = redraw;
        if (redraw || !node.Surface!.HasContent)
            node.Surface!.RequestRender((IGpuSurfaceContent)options.Key);
        return true;
    }

    public CompositionGroup Group(scoped in GroupOptions options)
    {
        options.Validate();
        if(!recording) throw new InvalidOperationException("Groups require an open frame.");
        // Arbitrary canvas clips flatten graphics into the current drawing surface.
        if(!options.ViewBacking || state.Clipped)
        {
            groups.Push(new(current,null,state,saves.Count));
            Save(); Transform(options.Transform); SurfaceEffects.Draw(this,in options);
            if(options.ClipToBounds) { BeginPath(); RoundRect(options.Bounds!.Value,options.CornerRadius); Clip(); BeginPath(); }
            return new(this,true);
        }
        var child = (GraphicsNode)GetOrCreateNode(options.Key, gpu: false);
        var redraw = NeedsUpdate(child, in options) || !child.Inherited.Same(current.State);
        ConfigureNode(child, in options);
        groups.Push(new(current,child,state,saves.Count));
        if(redraw)
        {
            child.BeginUpdate(); child.Inherited.CopyFrom(current.State); child.State.CopyFrom(current.State);
        }
        else child.Updating=false;
        current=child;
        state.Matrix=AffineTransform.Identity; state.Clipped=false;
        if(redraw) { SetTransform(AffineTransform.Identity); BeginPath(); }
        return new(this,redraw);
    }

    private Node GetOrCreateNode(object key, bool gpu)
    {
        if(!current.Updating) throw new InvalidOperationException("Cannot declare children of a reused group.");
        if(current.ByKey.TryGetValue(key, out var child))
        {
            if(child.Seen==frame) throw new InvalidOperationException("Duplicate sibling group key in the same frame.");
            if((child is GpuNode) != gpu)
            {
                child.Dispose();
                current.ByKey.Remove(key);
                child = null;
            }
        }
        if(child is null)
        {
            child = gpu ? gpuNodeFactory() : graphicsNodeFactory();
            current.ByKey.Add(key, child);
        }
        child.Seen=frame;
        return child;
    }

    private bool NeedsUpdate(Node node, in GroupOptions options) =>
        options.HasUpdates || !node.Valid || node.RasterBounds!=options.Bounds!.Value || node.Scale!=scale;

    private void ConfigureNode(Node node, in GroupOptions options)
    {
        node.Options=options; node.Placement=state.Matrix*options.Transform;
        node.RasterBounds=options.Bounds!.Value; node.Scale=scale;
        current.AddChild(node);
    }

    public void EndGroup()
    {
        if(!groups.TryPeek(out var group)) throw new InvalidOperationException("No group to close.");
        if(group.Child is null)
        {
            if(saves.Count!=group.SaveCount+1) throw new InvalidOperationException("Unbalanced saves inside group.");
            Restore(); groups.Pop(); return;
        }
        if(saves.Count!=group.SaveCount) throw new InvalidOperationException("Unbalanced saves inside group.");
        groups.Pop(); current=group.Parent; state=group.State; RestoreTextState();
    }
    private void RestoreTextState()
    {
        measure.SetFont(state.Font);
        measure.TextAlign=state.TextAlign;
        measure.TextBaseline=state.TextBaseline;
    }
    private struct PaintState
    {
        public AffineTransform Matrix;
        public bool Clipped;
        public Font Font;
        public TextAlign TextAlign;
        public TextBaseline TextBaseline;
    }
    private readonly record struct SavedState(GraphicsNode Node,PaintState State,int Count);
    private readonly record struct GroupFrame(GraphicsNode Parent,GraphicsNode? Child,PaintState State,int SaveCount);
}
