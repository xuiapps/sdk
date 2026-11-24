using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdsfld(Instruction.Token instruction, int offset)
    {
        var identity = TimeSpanZero.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType)
            ?? throw new NotSupportedException("Static field storage is not implemented except TimeSpan.Zero.");
        FrameAddress(Frame.Values[offset]);
        writer.I32Const(0).I32Const(Frame.ValueSize(identity)).MemoryFill();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle, identity));
    }
}
