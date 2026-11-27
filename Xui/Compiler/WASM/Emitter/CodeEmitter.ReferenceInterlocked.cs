using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryReferenceInterlocked(Instruction.Token instruction, int offset)
    {
        if (ReferenceInterlocked.Read(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is not { } signature) return false;
        var value = method.Storage.Bind(signature.Result);
        bool compare = signature.Parameters.Length == 3;
        var replacement = value.TypeCode == System.Reflection.Metadata.SignatureTypeCode.String
            ? method.EqualityTemporaries[value].Left : method.CompareExchangeReplacement;
        var original = value.TypeCode == System.Reflection.Metadata.SignatureTypeCode.String
            ? method.EqualityTemporaries[value].Right : method.CompareExchangeOriginal;
        if (compare)
        {
            PopAssignable(value, instruction.Code, offset);
            writer.LocalSet(method.GcSource);
        }
        PopAssignable(value, instruction.Code, offset);
        writer.LocalSet(replacement);
        PopAssignable(value with { IsByReference = true }, instruction.Code, offset);
        writer.LocalSet(method.GcI32).LocalSet(method.GcOwner);

        // Single-agent operation, with no suspension between reading and replacing the slot.
        writer.LocalGet(method.GcOwner).LocalGet(method.GcI32)
            .Call(method.Storage.ReferenceAccessor(value, store: false));
        if (compare) writer.LocalTee(original).LocalGet(method.GcSource).RefEq().If();
        else writer.LocalSet(original);
        writer.LocalGet(method.GcOwner).LocalGet(method.GcI32).LocalGet(replacement)
            .Call(method.Storage.ReferenceAccessor(value, store: true));
        if (compare) writer.End();
        writer.LocalGet(original);
        stack.Push(new(value.StackType, value.Identity, array: value.Array));
        return true;
    }
}
