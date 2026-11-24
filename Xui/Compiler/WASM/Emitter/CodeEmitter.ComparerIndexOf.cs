using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryComparerIndexOf(DefaultEquality access, Instruction.Token instruction, int offset)
    {
        var element = method.Storage.Bind(access.Element);
        var shape = method.Storage.ArrayShape(new(access.Element));
        if (shape.StructElements) throw new NotSupportedException("Default comparer search for struct elements is not implemented.");
        var (left, right) = method.EqualityTemporaries[element];
        bool hasCount = access.Signature.Parameters.Length == 4;
        if (hasCount) { Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.EqualitySearchEnd); }
        if (access.Signature.Parameters.Length >= 3) { Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.ArrayIndex); }
        else writer.I32Const(0).LocalSet(method.ArrayIndex);
        PopAssignable(element, instruction.Code, offset); writer.LocalSet(right);
        var array = Pop(instruction.Code, offset); writer.LocalSet(method.ArrayReference);
        if (!array.IsNull && array.Array != shape.Signature) throw new InvalidProgramException("Default comparer search requires its exact vector type.");
        if (access.Signature.HasThis)
        {
            var receiver = Pop(instruction.Code, offset);
            if (receiver.IsByReference || receiver.Identity is not null && receiver.Identity != access.Owner)
                throw new InvalidProgramException("Default comparer search requires its declared receiver.");
            writer.RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
        }
        CheckCopyNull(method.ArrayReference, "array");
        CheckCopyNegative(method.ArrayIndex, "startIndex");
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().LocalSet(method.GcI32);
        writer.LocalGet(method.ArrayIndex).LocalGet(method.GcI32).I32GtU().If();
        ThrowCopyArgument(ExceptionKind.ArgumentOutOfRangeException, "startIndex"); writer.End();
        if (!hasCount) writer.LocalGet(method.GcI32).LocalGet(method.ArrayIndex).I32Sub().LocalSet(method.EqualitySearchEnd);
        CheckCopyNegative(method.EqualitySearchEnd, "count");
        writer.LocalGet(method.EqualitySearchEnd).LocalGet(method.GcI32).LocalGet(method.ArrayIndex).I32Sub().I32GtU().If();
        ThrowCopyArgument(ExceptionKind.ArgumentOutOfRangeException, "count"); writer.End();
        writer.LocalGet(method.EqualitySearchEnd).LocalGet(method.ArrayIndex).I32Add().LocalSet(method.EqualitySearchEnd);
        writer.I32Const(-1).LocalSet(method.EqualitySearchResult);
        writer.Block(method.Storage.EmptyBlock).Loop(method.Storage.EmptyBlock);
        writer.LocalGet(method.ArrayIndex).LocalGet(method.EqualitySearchEnd).I32LtU().I32Eqz().If().Br(new(2)).End();
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex).ArrayGet(shape.Type);
        if (element.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
        writer.LocalSet(left);
        CompareEquality(access, left, right);
        writer.If().LocalGet(method.ArrayIndex).LocalSet(method.EqualitySearchResult).Br(new(2)).End();
        writer.LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0));
        writer.End().End().LocalGet(method.EqualitySearchResult);
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
