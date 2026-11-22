using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitClt(int offset)
    {
        var right = Pop(OpCode.Clt, offset);
        Pop(right.TypeCode, OpCode.Clt, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32LtS(); break;
            case SignatureTypeCode.Int64:
                writer.I64LtS(); break;
            case SignatureTypeCode.Double:
                writer.F64Lt(); break;
            default:
                throw new NotSupportedException($"Unsupported Clt operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
