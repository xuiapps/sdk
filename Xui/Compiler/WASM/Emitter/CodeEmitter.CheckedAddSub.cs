using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCheckedAddSub(OpCode code, int offset)
    {
        var b = Pop(code, offset); var a = Pop(code, offset);
        bool Integer(MsilStackEntry value) => !value.IsByReference && value.Identity is null &&
            value.TypeCode is SignatureTypeCode.Int32 or SignatureTypeCode.Int64 or SignatureTypeCode.IntPtr;
        bool wide = b.TypeCode == SignatureTypeCode.Int64;
        if (!Integer(a) || !Integer(b) || wide != (a.TypeCode == SignatureTypeCode.Int64))
            throw new InvalidProgramException("Checked arithmetic requires compatible integer operands.");
        var category = a.TypeCode == SignatureTypeCode.IntPtr || b.TypeCode == SignatureTypeCode.IntPtr ? SignatureTypeCode.IntPtr : b.TypeCode;
        var (left, right) = method.EqualityTemporaries[new(category)];
        var result = wide ? method.GcI64 : method.GcI32;
        bool subtract = code is OpCode.Sub_ovf or OpCode.Sub_ovf_un;
        bool unsigned = code is OpCode.Add_ovf_un or OpCode.Sub_ovf_un;
        writer.LocalSet(right).LocalSet(left).LocalGet(left).LocalGet(right);
        if (wide) { if (subtract) writer.I64Sub(); else writer.I64Add(); }
        else { if (subtract) writer.I32Sub(); else writer.I32Add(); }
        writer.LocalSet(result);
        if (unsigned)
        {
            writer.LocalGet(subtract ? left : result).LocalGet(subtract ? right : left);
            if (wide) writer.I64LtU(); else writer.I32LtU();
        }
        else
        {
            // Add: (a ^ result) & (b ^ result); subtract: (a ^ b) & (a ^ result).
            // A set sign bit means the mathematical result crossed the signed bound.
            writer.LocalGet(left).LocalGet(subtract ? right : result);
            if (wide) writer.I64Xor(); else writer.I32Xor();
            writer.LocalGet(subtract ? left : right).LocalGet(result);
            if (wide) writer.I64Xor().I64And().I64Const(0).I64LtS();
            else writer.I32Xor().I32And().I32Const(0).I32LtS();
        }
        writer.If(); ThrowException(ExceptionKind.OverflowException); writer.End().LocalGet(result);
        stack.Push(new(category));
    }
}
