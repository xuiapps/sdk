using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>A GC object's flat fields. Nested value types contribute leaves, never child objects.</summary>
internal sealed class GcShape(TypeIdentity identity)
{
    public TypeIdentity Identity { get; } = identity;
    public TypeIndex Type { get; set; }
    public List<GcLeaf> Leaves { get; } = new();
    public Dictionary<FieldIdentity, GcField> Fields { get; } = new();
}
