using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCeq(int offset)
    {
        if (TryReferenceComparison(OpCode.Ceq, offset, false))
        {
            stack.Push(new(SignatureTypeCode.Int32)); return;
        }
        var right = Pop(OpCode.Ceq, offset);
        Pop(right.TypeCode, OpCode.Ceq, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Eq(); break;
            case SignatureTypeCode.Int64:
                writer.I64Eq(); break;
            case SignatureTypeCode.Double:
                writer.F64Eq(); break;
            default:
                throw new NotSupportedException($"Unsupported Ceq operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
