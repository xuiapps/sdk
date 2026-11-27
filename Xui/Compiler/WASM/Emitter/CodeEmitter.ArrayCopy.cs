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
        if (copy.ByteOffsets) return EmitBufferBlockCopy(instruction, offset);
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
        if (!signature.IsVector) throw new NotSupportedException("Array.Copy on rectangular arrays is not implemented.");
        if (sourceArray.Array is { } sourceType && sourceType != signature || destination.Array is { } destinationType && destinationType != signature)
            throw new NotSupportedException("Array.Copy requires the same exact vector element type.");
        var shape = method.Storage.ArrayShape(signature);

        CheckCopyNegative(method.CopyLength, "length");
        CheckCopyNegative(method.CopySourceIndex, "sourceIndex");
        CheckCopyNegative(method.CopyDestinationIndex, "destinationIndex");
        CheckCopyRange(method.GcSource, method.CopySourceIndex, shape);
        CheckCopyRange(method.GcOwner, method.CopyDestinationIndex, shape);
        if (shape.StructElements) { CopyStructElements(shape); return true; }
        // array.copy has memmove semantics even when source and destination overlap.
        writer.LocalGet(method.GcOwner).RefCast(shape.Type).LocalGet(method.CopyDestinationIndex)
            .LocalGet(method.GcSource).RefCast(shape.Type).LocalGet(method.CopySourceIndex)
            .LocalGet(method.CopyLength).ArrayCopy(shape.Type, shape.Type);
        return true;
    }

    private void CopyStructElements(ArrayShape array)
    {
        var element = method.Storage.Shape(array.Element.Identity!.Value);
        writer.LocalGet(method.GcSource).LocalGet(method.GcOwner).RefEq()
            .LocalGet(method.CopyDestinationIndex).LocalGet(method.CopySourceIndex).I32GtU().I32And().If();
        writer.I32Const(-1).LocalSet(method.GcI32);
        writer.LocalGet(method.CopySourceIndex).LocalGet(method.CopyLength).I32Add().I32Const(1).I32Sub().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.CopyLength).I32Add().I32Const(1).I32Sub().LocalSet(method.CopyDestinationIndex);
        writer.Else().I32Const(1).LocalSet(method.GcI32).End();
        writer.Block(method.Storage.EmptyBlock).Loop().LocalGet(method.CopyLength).I32Eqz().BrIf(new(1));
        // Copy fields into the existing element holder: outstanding ldelema refs keep aliasing it.
        for (int i = 0; i < element.Leaves.Count; i++)
            writer.LocalGet(method.GcOwner).RefCast(array.Type).LocalGet(method.CopyDestinationIndex).ArrayGet(array.Type).RefCast(element.Type)
                .LocalGet(method.GcSource).RefCast(array.Type).LocalGet(method.CopySourceIndex).ArrayGet(array.Type).RefCast(element.Type)
                .StructGet(element.Type, new((uint)i)).StructSet(element.Type, new((uint)i));
        writer.LocalGet(method.CopySourceIndex).LocalGet(method.GcI32).I32Add().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.GcI32).I32Add().LocalSet(method.CopyDestinationIndex);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(method.CopyLength).Br(new(0)).End().End();
    }

    private void CheckCopyNull(LocalIndex array, string name)
    {
        writer.LocalGet(array).RefIsNull().If();
        ThrowArgumentException(ExceptionKind.ArgumentNullException, name);
        writer.End();
    }

    private void CheckCopyNegative(LocalIndex index, string name)
    {
        writer.LocalGet(index).I32Const(0).I32LtS().If();
        ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, name);
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

    private void ThrowArgumentException(ExceptionKind kind, string name, string? message = null)
    {
        EmitString(message ?? ExceptionTypes.Message(kind)); writer.LocalSet(method.GcExtern);
        EmitString(name); writer.LocalSet(method.ExceptionParamName);
        WriteArgumentException(kind);
        writer.Throw(method.Storage.ExceptionTag);
    }
}
