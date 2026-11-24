using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_i4(int offset)
    {
        var address = Pop(OpCode.Ldind_i4, offset);
        if (!address.IsByReference || address.TypeCode is not (SignatureTypeCode.Int32 or SignatureTypeCode.UInt32))
            throw new InvalidProgramException("ldind.i4 requires an Int32/UInt32 managed address.");
        if (address.Location is { } location)
            writer.RefCast(location.Owner.Type).StructGet(location.Owner.Type, new((uint)location.First));
        else writer.I32Load();
        stack.Push(new(SignatureTypeCode.Int32));
    }
}
