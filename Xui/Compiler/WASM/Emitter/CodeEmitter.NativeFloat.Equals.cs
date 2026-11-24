using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNativeFloatEquals(Instruction.Token instruction, int offset)
    {
        Pop(SignatureTypeCode.Double, instruction.Code, offset);
        writer.F32DemoteF64().LocalSet(method.NativeFloatTemporary);
        var address = Pop(instruction.Code, offset);
        if (!address.IsByReference || address.TypeCode != SignatureTypeCode.Single &&
            (address.Identity is not { } type || !NativeFloat.Is(type)))
            throw new InvalidProgramException("NFloat.Equals requires an NFloat managed address.");
        if (address.Location is { } field)
            writer.RefCast(field.Owner.Type).StructGet(field.Owner.Type, new((uint)field.First));
        else writer.F32Load();
        writer.F64PromoteF32().LocalSet(method.GcF64);
        writer.LocalGet(method.GcF64).LocalGet(method.NativeFloatTemporary).F64PromoteF32().F64Eq()
            .LocalGet(method.GcF64).LocalGet(method.GcF64).F64Eq().I32Eqz()
            .LocalGet(method.NativeFloatTemporary).LocalGet(method.NativeFloatTemporary).F32Ne().I32And().I32Or();
        stack.Push(new(SignatureTypeCode.Int32));
    }
}
