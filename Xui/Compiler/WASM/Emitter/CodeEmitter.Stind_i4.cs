using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_i4(int offset)
    {
        Pop(SignatureTypeCode.Int32, OpCode.Stind_i4, offset);
        var address = Pop(OpCode.Stind_i4, offset);
        if (!address.IsByReference || address.TypeCode is not (SignatureTypeCode.Int32 or SignatureTypeCode.UInt32))
            throw new InvalidProgramException("stind.i4 requires an Int32/UInt32 managed address.");
        if (address.Location is { } location)
            writer.LocalSet(method.GcI32).RefCast(location.Owner.Type).LocalGet(method.GcI32).StructSet(location.Owner.Type, new((uint)location.First));
        else writer.I32Store();
    }
}
