using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryGcArrayAllocation(Instruction.Token instruction, int offset)
    {
        var allocation = GcArrayAllocation.Read(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (allocation is null) return false;
        Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        writer.If(); ThrowException(ExceptionKind.NotSupportedException); writer.End();
        Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        writer.LocalTee(method.ArrayIndex).I32Const(0).I32LtS().If();
        ThrowException(ExceptionKind.OverflowException);
        writer.End().LocalGet(method.ArrayIndex);
        var shape = method.Storage.ArrayShape(allocation.Signature.Result.Array!);
        // AllocateUninitializedArray permits initialization. GC storage cannot be
        // pinned or exposed as a linear-memory pointer, regardless of element type.
        NewArray(shape);
        stack.Push(new(SignatureTypeCode.SZArray, array: shape.Signature));
        return true;
    }
}
