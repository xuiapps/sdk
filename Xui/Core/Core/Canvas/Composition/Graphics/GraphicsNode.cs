using Xui.Core.Canvas.Composition.Commands;

namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>A retained 2D drawing surface with recorded canvas commands and ordered composition children.</summary>
/// <remarks>Command buffers and reconciliation caches belong here; shared presentation is provided by Node. Platforms may derive from this class to own their
/// native resources and release them in DisposeCore. Native backends replay Commands when needed.</remarks>
public class GraphicsNode : Node
{
    internal readonly Dictionary<object,Node> ByKey=new(ReferenceEqualityComparer.Instance);
    internal readonly CanvasCommandBuffer State=new();
    internal readonly List<object> Removed=new();
    /// <summary>Inherited canvas state replayed before this surface's own commands.</summary>
    public readonly CanvasCommandBuffer Inherited=new();
    /// <summary>Recorded 2D drawing content owned by this surface.</summary>
    public readonly CanvasCommandBuffer Commands=new();
    private readonly List<Node> children = new();
    /// <inheritdoc/>
    public override int Count => children.Count;
    /// <inheritdoc/>
    public override Node this[int index] => children[index];
    internal void AddChild(Node child) => children.Add(child);
    /// <summary>Cached, omitted children. Backends detach these surfaces until they are declared again.</summary>
    public readonly List<Node> HiddenChildren=new();
    /// <summary>Whether the command stream contains visible drawing operations.</summary>
    public bool HasDrawing { get; internal set; }
    internal void BeginUpdate() { Updating=true; HasDrawing=false; Commands.Clear(); children.Clear(); }
    internal override void Prune(long frame)
    {
        if(!Updating) return;
        Removed.Clear(); HiddenChildren.Clear();
        foreach(var (key,child) in ByKey)
        {
            if(child.Seen==frame) child.Prune(frame);
            else if(child.Options.CacheWhenHidden) HiddenChildren.Add(child);
            else Removed.Add(key);
        }
        foreach(var key in Removed) { ByKey[key].Dispose(); ByKey.Remove(key); }
        while(HiddenChildren.Count>8)
        {
            var oldest=0;
            for(var i=1;i<HiddenChildren.Count;i++)
                if(HiddenChildren[i].Seen<HiddenChildren[oldest].Seen) oldest=i;
            var child=HiddenChildren[oldest];
            ByKey.Remove(child.Options.Key); child.Dispose(); HiddenChildren.RemoveAt(oldest);
        }
        Removed.Clear();
    }
    internal override void Accept() { Valid=true; foreach(var child in children) child.Accept(); Updating=false; }
    internal override void Invalidate() { Valid=false; foreach(var child in ByKey.Values) child.Invalidate(); }
    /// <inheritdoc/>
    protected override void DisposeCore()
    {
        base.DisposeCore();
        foreach(var child in ByKey.Values) child.Dispose();
        ByKey.Clear(); children.Clear(); HiddenChildren.Clear();
        Commands.Dispose(); Inherited.Dispose(); State.Dispose();
    }
}
