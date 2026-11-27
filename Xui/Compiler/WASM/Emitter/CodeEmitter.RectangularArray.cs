using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryRectangularArray(Instruction.Token instruction, int offset)
    {
        var call = RectangularArray.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (call is null) return false;
        if (call.ArrayType is not { Array: { } signature }) { EmitArrayDimensions(call, instruction.Code, offset); return true; }
        var shape = method.Storage.ArrayShape(signature);
        var data = shape.Backing!;
        if (call.Operation == ".ctor")
        {
            if (instruction.Code != OpCode.Newobj) throw new NotSupportedException("Array construction requires newobj.");
            ReadRectangularIndices(signature.Rank, instruction.Code, offset);
            writer.I32Const(0).LocalSet(method.GcI32);
            for (int i = 0; i < signature.Rank; i++)
            {
                writer.LocalGet(method.RectangularIndices[i]).I32Const(0).I32LtS().If();
                ThrowException(ExceptionKind.OverflowException); writer.End();
                writer.LocalGet(method.GcI32).LocalGet(method.RectangularIndices[i]).I32Eqz().I32Or().LocalSet(method.GcI32);
            }
            // An empty dimension makes the whole array empty, even with large other dimensions.
            writer.I64Const(0).LocalSet(method.GcI64).LocalGet(method.GcI32).I32Eqz().If();
            writer.I64Const(1).LocalSet(method.GcI64);
            for (int i = 0; i < signature.Rank; i++)
            {
                writer.LocalGet(method.GcI64).LocalGet(method.RectangularIndices[i]).I64ExtendI32U().I64Mul().LocalTee(method.GcI64)
                    .I64Const(int.MaxValue).I64GtU().If();
                ThrowException(ExceptionKind.OverflowException); writer.End();
            }
            writer.End().LocalGet(method.GcI64).I32WrapI64();
            NewArray(data); writer.LocalSet(method.ArrayReference);
            for (int i = 0; i < signature.Rank; i++) writer.LocalGet(method.RectangularIndices[i]);
            writer.LocalGet(method.ArrayReference).StructNew(shape.Type);
            TagArray(shape);
            stack.Push(new(SignatureTypeCode.Array, array: signature));
            return true;
        }
        if (call.Operation == "Set")
        {
            PopAssignable(data.Element, instruction.Code, offset);
            writer.LocalSet(GcScratch(data.Element));
        }
        ReadRectangularIndices(signature.Rank, instruction.Code, offset);
        PopAssignable(method.Storage.Bind(call.ArrayType.Value), instruction.Code, offset);
        writer.LocalSet(method.ArrayReference).LocalGet(method.ArrayReference); CheckExceptionNull(); writer.Drop();
        writer.I32Const(0).LocalSet(method.ArrayIndex);
        for (int i = 0; i < signature.Rank; i++)
        {
            writer.LocalGet(method.RectangularIndices[i]).LocalGet(method.ArrayReference).RefCast(shape.Type)
                .StructGet(shape.Type, new((uint)i)).I32LtU().I32Eqz().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException); writer.End();
            writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(shape.Type).StructGet(shape.Type, new((uint)i))
                .I32Mul().LocalGet(method.RectangularIndices[i]).I32Add().LocalSet(method.ArrayIndex);
        }
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).StructGet(shape.Type, new((uint)signature.Rank)).LocalSet(method.ArrayReference);
        if (call.Operation == "Set") StoreArrayElement(data, data.Element);
        else LoadArrayElement(data, instruction.Code, offset, call.Operation == "Address");
        return true;
    }

    private void ReadRectangularIndices(int count, OpCode code, int offset)
    {
        for (int i = count - 1; i >= 0; i--)
        { Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(method.RectangularIndices[i]); }
    }

    private void EmitArrayDimensions(RectangularArray call, OpCode code, int offset)
    {
        bool dimension = call.Signature.Parameters.Length != 0;
        if (dimension) { Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(method.RectangularIndices[0]); }
        var receiver = Pop(code, offset);
        var signature = receiver.Array ?? throw new NotSupportedException("Array dimensions require an exact array type.");
        var shape = method.Storage.ArrayShape(signature);
        writer.LocalSet(method.ArrayReference).LocalGet(method.ArrayReference); CheckExceptionNull(); writer.Drop();
        if (dimension)
        {
            writer.LocalGet(method.RectangularIndices[0]).I32Const(signature.Rank).I32LtU().I32Eqz().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException); writer.End();
        }
        if (call.Operation == "get_Rank") writer.I32Const(signature.Rank);
        else if (call.Operation == "GetLowerBound") writer.I32Const(0);
        else if (signature.IsVector) writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen();
        else if (call.Operation == "get_Length")
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).StructGet(shape.Type, new((uint)signature.Rank)).RefCast(shape.Backing!.Type).ArrayLen();
        else
        {
            writer.I32Const(0).LocalSet(method.ArrayIndex);
            for (int i = 0; i < signature.Rank; i++)
                writer.LocalGet(method.RectangularIndices[0]).I32Const(i).I32Eq().If()
                    .LocalGet(method.ArrayReference).RefCast(shape.Type).StructGet(shape.Type, new((uint)i)).LocalSet(method.ArrayIndex).End();
            writer.LocalGet(method.ArrayIndex);
        }
        if (call.Operation == "GetUpperBound") writer.I32Const(1).I32Sub();
        stack.Push(new(SignatureTypeCode.Int32));
    }
}
