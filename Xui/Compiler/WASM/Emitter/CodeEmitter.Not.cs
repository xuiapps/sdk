using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNot(int offset)
    {
        var value = Pop(OpCode.Not, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Const(-1).I32Xor(); break;
            case SignatureTypeCode.Int64:
                writer.I64Const(-1).I64Xor(); break;
            default:
                throw new NotSupportedException($"Unsupported Not operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(value);
    }
}
