using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_r4(int offset)
    {
        var value = Pop(OpCode.Conv_r4, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr: writer.F32ConvertI32S().F64PromoteF32(); break;
            case SignatureTypeCode.Int64: writer.F32ConvertI64S().F64PromoteF32(); break;
            case SignatureTypeCode.Double: writer.F32DemoteF64().F64PromoteF32(); break;
            default:
                throw new NotSupportedException($"Unsupported Conv_r4 operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Double));
    }
}
