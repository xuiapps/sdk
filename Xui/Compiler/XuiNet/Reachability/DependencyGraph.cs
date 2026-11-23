using System.Collections.ObjectModel;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>The frozen requirements reachable from a set of roots. Keep their PE assemblies alive while inspecting identities.</summary>
public sealed class DependencyGraph
{
    private readonly HashSet<Dependency> membership;
    private readonly IReadOnlyDictionary<TypeIdentity, IReadOnlyList<VirtualSlot>> slots;

    internal DependencyGraph(IEnumerable<Dependency> nodes, IEnumerable<DependencyEdge> edges, DispatchTable dispatch)
    {
        Nodes = Array.AsReadOnly(nodes.ToArray());
        Edges = Array.AsReadOnly(edges.ToArray());
        membership = Nodes.ToHashSet();
        Methods = Array.AsReadOnly(Nodes.OfType<Dependency.Method>().Select(node => node.Identity).ToArray());
        slots = new ReadOnlyDictionary<TypeIdentity, IReadOnlyList<VirtualSlot>>(
            new Dictionary<TypeIdentity, IReadOnlyList<VirtualSlot>>(dispatch.Types));
    }

    /// <summary>Interned requirements in deterministic discovery order.</summary>
    public IReadOnlyList<Dependency> Nodes { get; }
    /// <summary>Dependency reasons, including conditional virtual targets and instruction offsets.</summary>
    public IReadOnlyList<DependencyEdge> Edges { get; }
    /// <summary>Retained concrete methods, including abstract declarations used by dispatch signatures.</summary>
    public IReadOnlyList<MethodIdentity> Methods { get; }
    /// <summary>Whether a requirement was reached.</summary>
    public bool Contains(Dependency dependency) => membership.Contains(dependency);
    /// <summary>Whether host collection replaced this instruction.</summary>
    public bool IsRewritten(MethodIdentity method, int offset) => Contains(new Dependency.Rewrite(method, offset));
    /// <summary>The inherited slots of a retained class layout.</summary>
    public IReadOnlyList<VirtualSlot> VirtualSlots(TypeIdentity type) => slots.TryGetValue(type, out var result)
        ? result : throw new InvalidOperationException($"Class layout was not retained: {type}.");
}
