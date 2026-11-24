using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.Layout;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Compiler-owned stack layout. Reference fields contain four-byte table indices, not CLR pointers.</summary>
internal sealed record StackStructLayout(
    int Size,
    int Alignment,
    IReadOnlyDictionary<FieldDefinitionHandle, FieldLayout> Fields);
