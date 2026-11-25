using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitDivision(OpCode code, int offset)
    {
        var value = Pop(code, offset);
        Pop(value.TypeCode, code, offset);
        if (value.TypeCode == SignatureTypeCode.Double)
        {
            if (code == OpCode.Div) writer.F64Div();
            else if (code == OpCode.Rem) writer.Call(method.Storage.MathImports.Remainder);
            else throw new InvalidProgramException("Unsigned floating division and remainder are not valid MSIL.");
            stack.Push(value); return;
        }
        if (value.TypeCode is not (SignatureTypeCode.Int32 or SignatureTypeCode.Int64 or SignatureTypeCode.IntPtr))
            throw new NotSupportedException($"Unsupported division type: {value.TypeCode}.");
        bool wide = value.TypeCode == SignatureTypeCode.Int64;
        bool unsigned = code is OpCode.Div_un or OpCode.Rem_un;
        bool remainder = code is OpCode.Rem or OpCode.Rem_un;
        var (left, right) = method.EqualityTemporaries[new(value.TypeCode)];
        writer.LocalSet(right).LocalSet(left).LocalGet(right);
        if (wide) writer.I64Eqz(); else writer.I32Eqz();
        writer.If(); ThrowException(ExceptionKind.DivideByZeroException); writer.End();
        if (!unsigned)
        {
            writer.LocalGet(left);
            if (wide) writer.I64Const(long.MinValue).I64Eq().LocalGet(right).I64Const(-1).I64Eq();
            else writer.I32Const(int.MinValue).I32Eq().LocalGet(right).I32Const(-1).I32Eq();
            writer.I32And().If(); ThrowException(ExceptionKind.OverflowException); writer.End();
        }
        writer.LocalGet(left).LocalGet(right);
        if (wide)
        {
            if (remainder) { if (unsigned) writer.I64RemU(); else writer.I64RemS(); }
            else if (unsigned) writer.I64DivU(); else writer.I64DivS();
        }
        else
        {
            if (remainder) { if (unsigned) writer.I32RemU(); else writer.I32RemS(); }
            else if (unsigned) writer.I32DivU(); else writer.I32DivS();
        }
        stack.Push(value);
    }
}
