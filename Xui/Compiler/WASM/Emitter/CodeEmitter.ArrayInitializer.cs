using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitArrayInitializer(Instruction.Token instruction, int offset, StaticSpanCall data)
    {
        if (Pop(instruction.Code, offset).FieldToken != data.Field)
            throw new InvalidProgramException("InitializeArray requires its paired static field token.");
        writer.Drop();
        var array = Pop(instruction.Code, offset).Array ?? throw new NotSupportedException("InitializeArray requires an exact vector.");
        int size = StaticSpanCalls.ArrayElementSize(array.Element);
        if (size == 0) throw new NotSupportedException("InitializeArray requires primitive or enum elements.");
        var shape = method.Storage.ArrayShape(array);
        writer.LocalSet(method.ArrayReference);
        CheckCopyNull(method.ArrayReference, "array");
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32Const(data.ByteLength / size).I32GtU().If();
        ThrowException(ExceptionKind.ArgumentException); writer.End();
        writer.I32Const(0).LocalSet(method.ArrayIndex).Block(method.Storage.EmptyBlock).Loop();
        writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32LtU().I32Eqz().BrIf(new(1));
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex);
        writer.I32Const(method.Storage.StaticData.Address(data.Field)).LocalGet(method.ArrayIndex).I32Const(size).I32Mul().I32Add();
        switch (shape.Element.TypeCode)
        {
            case SignatureTypeCode.Boolean:
            case SignatureTypeCode.Byte: writer.I32Load8U(); break;
            case SignatureTypeCode.SByte: writer.I32Load8S(); break;
            case SignatureTypeCode.Char:
            case SignatureTypeCode.UInt16: writer.I32Load16U(); break;
            case SignatureTypeCode.Int16: writer.I32Load16S(); break;
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32: writer.I32Load(); break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64: writer.I64Load(); break;
            case SignatureTypeCode.Single: writer.F32Load(); break;
            case SignatureTypeCode.Double: writer.F64Load(); break;
            default: throw new NotSupportedException($"InitializeArray element: {shape.Element.TypeCode}.");
        }
        writer.ArraySet(shape.Type).LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0)).End().End();
    }
}
