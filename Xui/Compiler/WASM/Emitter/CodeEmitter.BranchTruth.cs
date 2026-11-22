using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBranchTruth(OpCode code, int offset, bool branchOnTrue)
    {
        var value = Pop(code, offset);
        switch (value.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr:
                if (!branchOnTrue) writer.I32Eqz();
                return;
            case SignatureTypeCode.Int64:
                writer.I64Eqz();
                break;
            case SignatureTypeCode.Object when value.IsNull:
            case SignatureTypeCode.TypeHandle when value.Identity is { } identity && method.Storage.IsClass(identity):
            case SignatureTypeCode.String:
                writer.RefIsNull();
                break;
            default:
                throw new NotSupportedException($"Unsupported {code} operand type {value.TypeCode} at IL_{offset:x4}.");
        }
        if (branchOnTrue) writer.I32Eqz();
    }
}
