using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class FormattingRuntime
{
    private bool usesUInt32, usesInt64, usesUInt64;
    private TypeIndex int64Type;
    private FunctionIndex uint32, int64, uint64;

    public void RequireInteger(SignatureTypeCode code)
    {
        switch (code)
        {
            case SignatureTypeCode.UInt32: usesUInt32 = true; break;
            case SignatureTypeCode.Int64: usesInt64 = true; break;
            case SignatureTypeCode.UInt64: usesUInt64 = true; break;
            case SignatureTypeCode.SByte: case SignatureTypeCode.Byte:
            case SignatureTypeCode.Int16: case SignatureTypeCode.UInt16:
            case SignatureTypeCode.Int32: UsesInt32 = true; break;
            default: throw new NotSupportedException($"Unsupported integer formatting type: {code}.");
        }
    }

    public FunctionIndex Integer(SignatureTypeCode code) => code switch
    {
        SignatureTypeCode.UInt32 => uint32,
        SignatureTypeCode.Int64 => int64,
        SignatureTypeCode.UInt64 => uint64,
        SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or SignatureTypeCode.Int32 => Int32,
        _ => throw new NotSupportedException($"Unsupported integer formatting type: {code}.")
    };
}
