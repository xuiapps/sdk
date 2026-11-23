using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // Integer conversions intentionally reject floating-point inputs until their overflow behavior is defined.
    private void ConvertToInt32(OpCode code, int offset)
    {
        var value = Pop(code, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr: break;
            case SignatureTypeCode.Int64: writer.I32WrapI64(); break;
            default:
                throw new NotSupportedException($"Unsupported {code} operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
