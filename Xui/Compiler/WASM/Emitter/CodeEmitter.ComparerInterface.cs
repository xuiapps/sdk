using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitDefaultInterfaceResult(DefaultEquality comparer, LocalIndex[] arguments)
    {
        if (comparer.Operation == "GetHashCode") { writer.LocalGet(arguments[1]); return; }
        var storage = method.Storage.Bind(comparer.Element);
        var left = arguments[1]; var right = arguments[2];
        if (storage.TypeCode == SignatureTypeCode.Single)
        {
            (left, right) = method.EqualityTemporaries[storage];
            writer.LocalGet(arguments[1]).F64PromoteF32().LocalSet(left);
            writer.LocalGet(arguments[2]).F64PromoteF32().LocalSet(right);
        }
        CompareEquality(comparer, left, right);
    }
}
