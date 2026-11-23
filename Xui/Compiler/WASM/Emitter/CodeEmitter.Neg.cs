using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNeg(int offset)
    {
        var value = Pop(OpCode.Neg, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Const(-1).I32Mul(); break;
            case SignatureTypeCode.Int64:
                writer.I64Const(-1).I64Mul(); break;
            case SignatureTypeCode.Double:
                writer.F64Neg(); break;
            default:
                throw new NotSupportedException($"Unsupported Neg operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(value);
    }
}
