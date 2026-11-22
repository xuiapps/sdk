using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitClt_un(int offset)
    {
        var right = Pop(OpCode.Clt_un, offset);
        Pop(right.TypeCode, OpCode.Clt_un, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32LtU(); break;
            case SignatureTypeCode.Int64:
                writer.I64LtU(); break;
            case SignatureTypeCode.Double:
                // Invert the ordered opposite comparison so NaN produces true.
                writer.F64Ge().I32Eqz(); break;
            default:
                throw new NotSupportedException($"Unsupported Clt_un operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
