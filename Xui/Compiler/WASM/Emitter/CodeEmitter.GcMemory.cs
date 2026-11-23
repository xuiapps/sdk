using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void LoadRaw(LocalVariable type)
    {
        switch (type.TypeCode)
        {
            case SignatureTypeCode.SByte: writer.I32Load8S(); break;
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Boolean: writer.I32Load8U(); break;
            case SignatureTypeCode.Int16: writer.I32Load16S(); break;
            case SignatureTypeCode.Char:
            case SignatureTypeCode.UInt16: writer.I32Load16U(); break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64: writer.I64Load(); break;
            case SignatureTypeCode.Single: writer.F32Load(); break;
            case SignatureTypeCode.Double: writer.F64Load(); break;
            default: writer.I32Load(); break;
        }
    }

    private void StoreRaw(LocalVariable type)
    {
        switch (type.TypeCode)
        {
            case SignatureTypeCode.SByte:
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Boolean: writer.I32Store8(); break;
            case SignatureTypeCode.Int16:
            case SignatureTypeCode.Char:
            case SignatureTypeCode.UInt16: writer.I32Store16(); break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64: writer.I64Store(); break;
            case SignatureTypeCode.Single: writer.F32Store(); break;
            case SignatureTypeCode.Double: writer.F64Store(); break;
            default: writer.I32Store(); break;
        }
    }
}
