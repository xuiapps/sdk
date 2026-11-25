using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryReferenceCompareExchange(Instruction.Token instruction, int offset)
    {
        if (ReferenceCompareExchange.Read(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is not { } signature) return false;
        var value = method.Storage.Bind(signature.Result);
        PopAssignable(value, instruction.Code, offset);
        writer.LocalSet(method.GcSource); // comparand
        PopAssignable(value, instruction.Code, offset);
        writer.LocalSet(method.CompareExchangeReplacement);
        PopAssignable(value with { IsByReference = true }, instruction.Code, offset);
        writer.LocalSet(method.GcI32).LocalSet(method.GcOwner); // selector, owner

        // Single-agent operation: accessors contain only WASM loads/stores, with no host calls or reentry.
        writer.LocalGet(method.GcOwner).LocalGet(method.GcI32)
            .Call(method.Storage.ReferenceAccessor(value, store: false)).LocalTee(method.CompareExchangeOriginal)
            .LocalGet(method.GcSource).RefEq().If()
            .LocalGet(method.GcOwner).LocalGet(method.GcI32).LocalGet(method.CompareExchangeReplacement)
            .Call(method.Storage.ReferenceAccessor(value, store: true)).End()
            .LocalGet(method.CompareExchangeOriginal);
        stack.Push(new(value.StackType, value.Identity, array: value.Array));
        return true;
    }
}
