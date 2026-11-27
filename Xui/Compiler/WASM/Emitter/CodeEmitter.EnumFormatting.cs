using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // Receives the enum's scalar storage value; returns a native JavaScript string.
    private void FormatEnum(TypeIdentity identity, LocalVariable storage)
    {
        if (storage.TypeCode is not (SignatureTypeCode.Int64 or SignatureTypeCode.UInt64))
        {
            if (storage.TypeCode is SignatureTypeCode.SByte or SignatureTypeCode.Int16 or SignatureTypeCode.Int32) writer.I64ExtendI32S();
            else writer.I64ExtendI32U();
        }
        EmitString(method.Storage.Formatting.RequireEnum(identity));
        writer.Call(method.Storage.Formatting.Enum);
    }
}
