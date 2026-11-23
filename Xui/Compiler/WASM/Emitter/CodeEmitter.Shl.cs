using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitShl(int offset)
    {
        Pop(SignatureTypeCode.Int32, OpCode.Shl, offset);
        var value = Pop(OpCode.Shl, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
                writer.I32Shl(); break;
            case SignatureTypeCode.Int64:
                // MSIL shift counts are int32; WASM i64 shifts require an i64 count.
                writer.I64ExtendI32U().I64Shl(); break;
            default:
                throw new NotSupportedException($"Unsupported Shl operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(value);
    }
}
