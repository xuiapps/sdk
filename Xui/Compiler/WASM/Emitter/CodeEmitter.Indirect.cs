using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitIndirect(OpCode code, int offset, bool store)
    {
        var address = store ? stack.ElementAt(1) : stack.Peek();
        if (!address.IsByReference) throw new InvalidProgramException("Indirect access requires a managed address.");
        var declared = new SignatureType(address.TypeCode, address.Identity, IsReferenceType: address.Identity is { IsValueType: false }, Array: address.Array);
        IndirectOperations.Validate(code, declared);
        var type = method.Storage.Bind(declared);
        if (store)
        {
            PopAssignable(type, code, offset);
            writer.LocalSet(GcScratch(type));
        }
        _ = Pop(code, offset);
        if (address.Location is { } location)
        {
            writer.RefCast(location.Owner.Type);
            if (store) { writer.LocalGet(GcScratch(type)); Narrow(type); writer.StructSet(location.Owner.Type, new((uint)location.First)); }
            else writer.StructGet(location.Owner.Type, new((uint)location.First));
        }
        else if (store) { writer.LocalGet(GcScratch(type)); Narrow(type); StoreRaw(type); }
        else LoadRaw(type);
        if (!store)
        {
            if (type.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
            if (code == OpCode.Ldind_i1) writer.I32Extend8S();
            if (code == OpCode.Ldind_u1) writer.I32Const(255).I32And();
            if (code == OpCode.Ldind_i2) writer.I32Extend16S();
            if (code == OpCode.Ldind_u2) writer.I32Const(65535).I32And();
            stack.Push(new(type.StackType, type.Identity, array: type.Array));
        }
    }
}
