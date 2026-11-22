using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitMul(int offset)
    {
        var right = Pop(OpCode.Mul, offset);
        Pop(right.TypeCode, OpCode.Mul, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Mul(); break;
            case SignatureTypeCode.Int64:
                writer.I64Mul(); break;
            case SignatureTypeCode.Double:
                writer.F64Mul(); break;
            default:
                throw new NotSupportedException($"Unsupported Mul operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(right);
    }
}
