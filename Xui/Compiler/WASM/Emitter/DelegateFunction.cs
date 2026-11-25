using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

internal sealed class DelegateFunction(TypeIndex type, ValueTypeCode[] locals)
{
    public TypeIndex Type { get; } = type;
    public ValueTypeCode[] Locals { get; } = locals;
    public FunctionIndex Function { get; set; }
}
