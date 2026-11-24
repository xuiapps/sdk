using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>A managed vector's element storage and concrete WASM GC array type.</summary>
internal sealed class ArrayShape(ArraySignature signature, LocalVariable element)
{
    public ArraySignature Signature { get; } = signature;
    public LocalVariable Element { get; } = element;
    public bool StructElements => Element.Identity is { IsValueType: true };
    public TypeIndex Type { get; set; }
}
