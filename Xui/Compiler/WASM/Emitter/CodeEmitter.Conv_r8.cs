using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_r8(int offset)
    {
        var value = Pop(OpCode.Conv_r8, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr: writer.F64ConvertI32S(); break;
            case SignatureTypeCode.Int64: writer.F64ConvertI64S(); break;
            case SignatureTypeCode.Double: break;
            default:
                throw new NotSupportedException($"Unsupported Conv_r8 operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Double));
    }
}
