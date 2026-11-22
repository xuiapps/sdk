using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_r_un(int offset)
    {
        var value = Pop(OpCode.Conv_r_un, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr: writer.F64ConvertI32U(); break;
            case SignatureTypeCode.Int64: writer.F64ConvertI64U(); break;
            default:
                throw new NotSupportedException($"Unsupported Conv_r_un operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Double));
    }
}
