using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitAdd(int offset)
    {
        var right = Pop(OpCode.Add, offset);
        Pop(right.TypeCode, OpCode.Add, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Add(); break;
            case SignatureTypeCode.Int64:
                writer.I64Add(); break;
            case SignatureTypeCode.Double:
                writer.F64Add(); break;
            default:
                throw new NotSupportedException($"Unsupported Add operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(right);
    }
}
