using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>A closed method's delegate adapter: all callbacks accept a leading GC-visible target.</summary>
internal sealed class DelegateTarget(MethodPlan method)
{
    public MethodPlan Method { get; } = method;
    public TypeIndex Type { get; set; }
    public FunctionIndex Function { get; set; }
    public uint TableSlot { get; set; }
}
