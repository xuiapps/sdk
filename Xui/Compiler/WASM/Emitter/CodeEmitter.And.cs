using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitAnd(int offset)
    {
        var right = Pop(OpCode.And, offset);
        Pop(right.TypeCode, OpCode.And, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32And(); break;
            case SignatureTypeCode.Int64:
                writer.I64And(); break;
            default:
                throw new NotSupportedException($"Unsupported And operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(right);
    }
}
