using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>A source field's contiguous range in the flattened GC object; these are field indices, not byte offsets.</summary>
public readonly record struct Field(
    FieldDefinitionHandle Handle,
    string Name,
    TypeIdentity DeclaringType,
    SignatureType DeclaredType,
    FieldIndex First,
    uint Count);
