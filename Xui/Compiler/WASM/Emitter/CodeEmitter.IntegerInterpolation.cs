using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void FormatInterpolatedInteger(SignatureTypeCode code)
    {
        var kind = code switch
        {
            SignatureTypeCode.SByte => FormattingValueKind.Signed8,
            SignatureTypeCode.Byte => FormattingValueKind.Unsigned8,
            SignatureTypeCode.Int16 => FormattingValueKind.Signed16,
            SignatureTypeCode.UInt16 => FormattingValueKind.Unsigned16,
            SignatureTypeCode.Int32 => FormattingValueKind.Signed32,
            SignatureTypeCode.UInt32 => FormattingValueKind.Unsigned32,
            SignatureTypeCode.Int64 => FormattingValueKind.Signed64,
            SignatureTypeCode.UInt64 => FormattingValueKind.Unsigned64,
            _ => throw new InvalidProgramException($"Not an integer interpolation type: {code}.")
        };
        writer.I32Const((int)kind);
        if (code is SignatureTypeCode.Int64 or SignatureTypeCode.UInt64) writer.LocalGet(method.GcI64);
        else
        {
            writer.LocalGet(method.GcI32);
            if (code is SignatureTypeCode.SByte or SignatureTypeCode.Int16 or SignatureTypeCode.Int32) writer.I64ExtendI32S();
            else writer.I64ExtendI32U();
        }
        writer.LocalGet(method.GcExtern).Call(method.Storage.Formatting.FormattedInteger);
        ReadFormattedResult();
    }
}
