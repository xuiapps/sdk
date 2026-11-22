using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCgt(int offset)
    {
        var right = Pop(OpCode.Cgt, offset);
        Pop(right.TypeCode, OpCode.Cgt, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32GtS(); break;
            case SignatureTypeCode.Int64:
                writer.I64GtS(); break;
            case SignatureTypeCode.Double:
                writer.F64Gt(); break;
            default:
                throw new NotSupportedException($"Unsupported Cgt operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
