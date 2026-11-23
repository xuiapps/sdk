using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCgt_un(int offset)
    {
        if (TryReferenceComparison(OpCode.Cgt_un, offset, true))
        {
            stack.Push(new(SignatureTypeCode.Int32)); return;
        }
        var right = Pop(OpCode.Cgt_un, offset);
        Pop(right.TypeCode, OpCode.Cgt_un, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32GtU(); break;
            case SignatureTypeCode.Int64:
                writer.I64GtU(); break;
            case SignatureTypeCode.Double:
                // Invert the ordered opposite comparison so NaN produces true.
                writer.F64Le().I32Eqz(); break;
            default:
                throw new NotSupportedException($"Unsupported Cgt_un operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
