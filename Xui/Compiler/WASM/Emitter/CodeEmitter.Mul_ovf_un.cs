using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitMul_ovf_un(int offset)
    {
        var rightValue = Pop(OpCode.Mul_ovf_un, offset);
        var leftValue = Pop(OpCode.Mul_ovf_un, offset);
        bool IsInteger(MsilStackEntry value) => !value.IsByReference && value.Identity is null &&
            value.TypeCode is SignatureTypeCode.Int32 or SignatureTypeCode.Int64 or SignatureTypeCode.IntPtr;
        bool wide = rightValue.TypeCode == SignatureTypeCode.Int64;
        if (!IsInteger(leftValue) || !IsInteger(rightValue) || wide != (leftValue.TypeCode == SignatureTypeCode.Int64))
            throw new InvalidProgramException("Unsigned checked multiplication requires compatible integer operands.");
        var category = leftValue.TypeCode == SignatureTypeCode.IntPtr || rightValue.TypeCode == SignatureTypeCode.IntPtr
            ? SignatureTypeCode.IntPtr : rightValue.TypeCode;
        var (left, right) = method.EqualityTemporaries[new(category)];
        writer.LocalSet(right).LocalSet(left);
        if (wide)
        {
            writer.LocalGet(right).I64Eqz().I32Eqz().If();
            writer.LocalGet(left).I64Const(-1).LocalGet(right).I64DivU().I64GtU().If();
            ThrowException(ExceptionKind.OverflowException); writer.End().End();
            writer.LocalGet(left).LocalGet(right).I64Mul();
        }
        else
        {
            writer.LocalGet(left).I64ExtendI32U().LocalGet(right).I64ExtendI32U().I64Mul().LocalSet(method.GcI64);
            writer.LocalGet(method.GcI64).I64Const(uint.MaxValue).I64GtU().If();
            ThrowException(ExceptionKind.OverflowException); writer.End();
            writer.LocalGet(method.GcI64).I32WrapI64();
        }
        stack.Push(new(category));
    }
}
