using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitOr(int offset)
    {
        var right = Pop(OpCode.Or, offset);
        Pop(right.TypeCode, OpCode.Or, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Or(); break;
            case SignatureTypeCode.Int64:
                writer.I64Or(); break;
            default:
                throw new NotSupportedException($"Unsupported Or operand type {right.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(right);
    }
}
