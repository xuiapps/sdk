using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void PopAssignable(LocalVariable storage, OpCode code, int offset)
    {
        var value = Pop(code, offset);
        if (!storage.IsByReference && storage.TypeCode == SignatureTypeCode.Object && IsObjectReference(value)) return;
        if (value.IsNull && (storage.TypeCode == SignatureTypeCode.String ||
            (storage.Identity is { } identity && method.Storage.IsClass(identity))))
        {
            if (storage.TypeCode == SignatureTypeCode.String) writer.Drop().RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.ExternRef);
            return;
        }
        bool compatibleIdentity = value.Identity == storage.Identity ||
            (!value.IsByReference && !storage.IsByReference && value.Identity is { } actual && storage.Identity is { } expected && actual.IsAssignableTo(expected));
        if (!compatibleIdentity || value.IsByReference != storage.IsByReference)
            throw new InvalidProgramException($"Evaluation stack identity mismatch for {code} at IL_{offset:x4}.");
        if (value.TypeCode != storage.StackType &&
            !(storage.StackType == SignatureTypeCode.IntPtr && value.TypeCode == SignatureTypeCode.Int32) &&
            !(storage.StackType == SignatureTypeCode.Int32 && value.TypeCode == SignatureTypeCode.IntPtr))
            throw new InvalidProgramException($"Evaluation stack type mismatch for {code} at IL_{offset:x4}; expected {storage.StackType}, found {value.TypeCode}.");
    }

    private bool IsObjectReference(MsilStackEntry value) => !value.IsByReference &&
        (value.IsNull || value.TypeCode == SignatureTypeCode.Object || value.Identity is { } identity && method.Storage.IsClass(identity));

    private void Narrow(LocalVariable storage)
    {
        switch (storage.TypeCode)
        {
            case SignatureTypeCode.SByte: writer.I32Extend8S(); break;
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Boolean: writer.I32Const(255).I32And(); break;
            case SignatureTypeCode.Int16: writer.I32Extend16S(); break;
            case SignatureTypeCode.UInt16:
            case SignatureTypeCode.Char: writer.I32Const(65535).I32And(); break;
            case SignatureTypeCode.Single: writer.F32DemoteF64(); break;
        }
    }
}
