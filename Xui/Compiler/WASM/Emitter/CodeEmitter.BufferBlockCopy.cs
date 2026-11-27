using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool EmitBufferBlockCopy(Instruction.Token instruction, int offset)
    {
        Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyLength);
        Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyDestinationIndex);
        var destination = Pop(instruction.Code, offset); writer.LocalSet(method.GcOwner);
        Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopySourceIndex);
        var source = Pop(instruction.Code, offset); writer.LocalSet(method.GcSource);
        CheckCopyNull(method.GcSource, "src"); CheckCopyNull(method.GcOwner, "dst");
        if (source.Array is null || destination.Array is null) { writer.Unreachable(); return true; } // A statically null argument has already thrown.
        int sourceSize = BufferElementSize(source.Array), destinationSize = BufferElementSize(destination.Array);
        if (sourceSize == 0 || destinationSize == 0) { ThrowException(ExceptionKind.ArgumentException); return true; }
        var from = method.Storage.ArrayShape(source.Array); var to = method.Storage.ArrayShape(destination.Array);
        CheckCopyNegative(method.CopySourceIndex, "srcOffset"); CheckCopyNegative(method.CopyDestinationIndex, "dstOffset"); CheckCopyNegative(method.CopyLength, "count");
        CheckBufferRange(method.GcSource, method.CopySourceIndex, from, sourceSize);
        CheckBufferRange(method.GcOwner, method.CopyDestinationIndex, to, destinationSize);
        if (sourceSize == 1 && source.Array == destination.Array)
        {
            writer.LocalGet(method.GcOwner).RefCast(to.Type).LocalGet(method.CopyDestinationIndex)
                .LocalGet(method.GcSource).RefCast(from.Type).LocalGet(method.CopySourceIndex).LocalGet(method.CopyLength).ArrayCopy(to.Type, from.Type);
            return true;
        }
        // GC elements are not pinned. Read/write their little-endian byte views without allocating a staging buffer.
        writer.LocalGet(method.GcSource).LocalGet(method.GcOwner).RefEq()
            .LocalGet(method.CopyDestinationIndex).LocalGet(method.CopySourceIndex).I32GtU().I32And().If();
        writer.I32Const(-1).LocalSet(method.GcI32);
        writer.LocalGet(method.CopySourceIndex).LocalGet(method.CopyLength).I32Add().I32Const(1).I32Sub().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.CopyLength).I32Add().I32Const(1).I32Sub().LocalSet(method.CopyDestinationIndex);
        writer.Else().I32Const(1).LocalSet(method.GcI32).End();
        writer.Block(method.Storage.EmptyBlock).Loop().LocalGet(method.CopyLength).I32Eqz().BrIf(new(1));
        LoadBufferWord(method.GcSource, method.CopySourceIndex, from, sourceSize);
        BufferShift(method.CopySourceIndex, sourceSize);
        writer.I64ShrU().I64Const(255).I64And().LocalSet(method.GcI64);
        writer.LocalGet(method.GcOwner).RefCast(to.Type);
        BufferIndex(method.CopyDestinationIndex, destinationSize);
        LoadBufferWord(method.GcOwner, method.CopyDestinationIndex, to, destinationSize);
        writer.I64Const(255); BufferShift(method.CopyDestinationIndex, destinationSize);
        writer.I64Shl().I64Const(-1).I64Xor().I64And().LocalGet(method.GcI64);
        BufferShift(method.CopyDestinationIndex, destinationSize);
        writer.I64Shl().I64Or();
        switch (to.Element.TypeCode)
        {
            case SignatureTypeCode.Double: writer.F64ReinterpretI64(); break;
            case SignatureTypeCode.Single: writer.I32WrapI64().F32ReinterpretI32(); break;
            case SignatureTypeCode.Int64 or SignatureTypeCode.UInt64: break;
            default: writer.I32WrapI64(); Narrow(to.Element); break;
        }
        writer.ArraySet(to.Type);
        writer.LocalGet(method.CopySourceIndex).LocalGet(method.GcI32).I32Add().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.GcI32).I32Add().LocalSet(method.CopyDestinationIndex);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(method.CopyLength).Br(new(0)).End().End();
        return true;
    }

    private static int BufferElementSize(ArraySignature array)
    {
        if (!array.IsVector) throw new NotSupportedException("Buffer.BlockCopy currently requires vectors.");
        if (array.Element.Identity is not null) return 0;
        return array.Element.Code switch {
            SignatureTypeCode.Boolean or SignatureTypeCode.Byte or SignatureTypeCode.SByte => 1,
            SignatureTypeCode.Char or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 => 2,
            SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Single or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr => 4,
            SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or SignatureTypeCode.Double => 8,
            _ => 0
        };
    }

    private void CheckBufferRange(LocalIndex array, LocalIndex index, ArrayShape shape, int size)
    {
        writer.LocalGet(index).I64ExtendI32U().LocalGet(method.CopyLength).I64ExtendI32U().I64Add()
            .LocalGet(array).RefCast(shape.Type).ArrayLen().I64ExtendI32U().I64Const(size).I64Mul().I64GtU().If();
        ThrowException(ExceptionKind.ArgumentException); writer.End();
    }

    private void BufferIndex(LocalIndex offset, int size) => writer.LocalGet(offset).I32Const(size).I32DivU();
    private void BufferShift(LocalIndex offset, int size) => writer.LocalGet(offset).I32Const(size - 1).I32And().I32Const(8).I32Mul().I64ExtendI32U();
    private void LoadBufferWord(LocalIndex array, LocalIndex offset, ArrayShape shape, int size)
    {
        writer.LocalGet(array).RefCast(shape.Type); BufferIndex(offset, size); writer.ArrayGet(shape.Type);
        switch (shape.Element.TypeCode)
        {
            case SignatureTypeCode.Double: writer.I64ReinterpretF64(); break;
            case SignatureTypeCode.Single: writer.I32ReinterpretF32().I64ExtendI32U(); break;
            case SignatureTypeCode.Int64 or SignatureTypeCode.UInt64: break;
            default: writer.I64ExtendI32U(); break;
        }
    }
}
