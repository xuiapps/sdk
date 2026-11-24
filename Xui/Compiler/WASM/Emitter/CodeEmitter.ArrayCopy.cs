using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryArrayCopy(Instruction.Token instruction, int offset)
    {
        var copy = ArrayCopy.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (copy is null) return false;
        Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyLength);
        if (copy.HasIndices) { Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyDestinationIndex); }
        else writer.I32Const(0).LocalSet(method.CopyDestinationIndex);
        var destination = Pop(instruction.Code, offset); writer.LocalSet(method.GcOwner);
        if (copy.HasIndices) { Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopySourceIndex); }
        else writer.I32Const(0).LocalSet(method.CopySourceIndex);
        var sourceArray = Pop(instruction.Code, offset); writer.LocalSet(method.GcSource);

        CheckCopyNull(method.GcSource, "sourceArray");
        CheckCopyNull(method.GcOwner, "destinationArray");
        var signature = sourceArray.Array ?? destination.Array;
        if (signature is null) { writer.Unreachable(); return true; } // Both operands are statically null.
        if (sourceArray.Array is { } sourceType && sourceType != signature || destination.Array is { } destinationType && destinationType != signature)
            throw new NotSupportedException("Array.Copy requires the same exact vector element type.");
        var shape = method.Storage.ArrayShape(signature);
        if (shape.StructElements) throw new NotSupportedException("Array.Copy for struct elements requires value-preserving field copies.");

        CheckCopyNegative(method.CopyLength, "length");
        CheckCopyNegative(method.CopySourceIndex, "sourceIndex");
        CheckCopyNegative(method.CopyDestinationIndex, "destinationIndex");
        CheckCopyRange(method.GcSource, method.CopySourceIndex, shape);
        CheckCopyRange(method.GcOwner, method.CopyDestinationIndex, shape);
        // array.copy has memmove semantics even when source and destination overlap.
        writer.LocalGet(method.GcOwner).RefCast(shape.Type).LocalGet(method.CopyDestinationIndex)
            .LocalGet(method.GcSource).RefCast(shape.Type).LocalGet(method.CopySourceIndex)
            .LocalGet(method.CopyLength).ArrayCopy(shape.Type, shape.Type);
        return true;
    }

    private void CheckCopyNull(LocalIndex array, string name)
    {
        writer.LocalGet(array).RefIsNull().If();
        ThrowCopyArgument(ExceptionKind.ArgumentNullException, name);
        writer.End();
    }

    private void CheckCopyNegative(LocalIndex index, string name)
    {
        writer.LocalGet(index).I32Const(0).I32LtS().If();
        ThrowCopyArgument(ExceptionKind.ArgumentOutOfRangeException, name);
        writer.End();
    }

    private void CheckCopyRange(LocalIndex array, LocalIndex index, ArrayShape shape)
    {
        writer.LocalGet(array).RefCast(shape.Type).ArrayLen().LocalSet(method.GcI32);
        writer.LocalGet(index).LocalGet(method.GcI32).I32GtU().If();
        ThrowException(ExceptionKind.ArgumentException); writer.End();
        writer.LocalGet(method.CopyLength).LocalGet(method.GcI32).LocalGet(index).I32Sub().I32GtU().If();
        ThrowException(ExceptionKind.ArgumentException); writer.End();
    }

    private void ThrowCopyArgument(ExceptionKind kind, string name)
    {
        EmitString(ExceptionTypes.Message(kind)); writer.LocalSet(method.GcExtern);
        EmitString(name); writer.LocalSet(method.ExceptionParamName);
        WriteArgumentException(kind);
        writer.Throw(method.Storage.ExceptionTag);
    }
}
