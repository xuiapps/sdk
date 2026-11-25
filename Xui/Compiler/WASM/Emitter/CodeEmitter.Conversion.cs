using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // Conv_i4 uses the modern .NET saturating conversion for floating-point inputs.
    private void ConvertToInt32(OpCode code, int offset)
    {
        var value = Pop(code, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr: break;
            case SignatureTypeCode.Int64: writer.I32WrapI64(); break;
            case SignatureTypeCode.Double when code == OpCode.Conv_i4: writer.I32TruncSatF64S(); break;
            default:
                throw new NotSupportedException($"Unsupported {code} operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
