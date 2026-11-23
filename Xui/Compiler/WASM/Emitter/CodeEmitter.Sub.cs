using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitSub(int offset)
    {
        var right = Pop(OpCode.Sub, offset);
        Pop(right.TypeCode, OpCode.Sub, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Sub(); break;
            case SignatureTypeCode.Int64:
                writer.I64Sub(); break;
            case SignatureTypeCode.Double:
                writer.F64Sub(); break;
            default:
                throw new NotSupportedException($"Unsupported Sub operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(right);
    }
}
