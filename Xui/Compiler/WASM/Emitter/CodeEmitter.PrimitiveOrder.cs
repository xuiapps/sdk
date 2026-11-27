using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void ComparePrimitiveOrder(LocalVariable storage, LocalIndex left, LocalIndex right)
    {
        if (storage.TypeCode is SignatureTypeCode.Byte or SignatureTypeCode.SByte or SignatureTypeCode.Char or
            SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or SignatureTypeCode.Boolean)
        {
            writer.LocalGet(left).LocalGet(right).I32Sub();
            return;
        }
        bool wide = storage.StackType == SignatureTypeCode.Int64;
        bool floating = storage.StackType == SignatureTypeCode.Double;
        bool unsigned = storage.TypeCode is SignatureTypeCode.UInt32 or SignatureTypeCode.UInt64 or SignatureTypeCode.UIntPtr;
        writer.LocalGet(left).LocalGet(right);
        if (floating) writer.F64Gt();
        else if (wide) { if (unsigned) writer.I64GtU(); else writer.I64GtS(); }
        else { if (unsigned) writer.I32GtU(); else writer.I32GtS(); }
        writer.LocalGet(left).LocalGet(right);
        if (floating) writer.F64Lt();
        else if (wide) { if (unsigned) writer.I64LtU(); else writer.I64LtS(); }
        else { if (unsigned) writer.I32LtU(); else writer.I32LtS(); }
        writer.I32Sub();
        if (floating)
        {
            // NaN compares below any number, and equal to another NaN.
            writer.LocalGet(right).LocalGet(right).F64Eq().I32Eqz().I32Add();
            writer.LocalGet(left).LocalGet(left).F64Eq().I32Eqz().I32Sub();
        }
    }
}
