using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitXor(int offset)
    {
        var right = Pop(OpCode.Xor, offset);
        Pop(right.TypeCode, OpCode.Xor, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Xor(); break;
            case SignatureTypeCode.Int64:
                writer.I64Xor(); break;
            default:
                throw new NotSupportedException($"Unsupported Xor operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(right);
    }
}
