using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_i8(int offset)
    {
        var value = Pop(OpCode.Conv_i8, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr: writer.I64ExtendI32S(); break;
            case SignatureTypeCode.Int64: break;
            default:
                throw new NotSupportedException($"Unsupported Conv_i8 operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int64));
    }
}
