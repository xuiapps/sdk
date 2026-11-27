using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdsfld(Instruction.Token instruction, int offset)
    {
        if (StringEmpty.Is(source, MetadataTokens.EntityHandle(instruction.Value)))
        {
            EmitString(""); stack.Push(new(SignatureTypeCode.String)); return;
        }
        if (StopwatchConstant.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is { } clock)
        {
            switch (clock.Code)
            {
                case SignatureTypeCode.Int64: writer.I64Const(StopwatchConstant.Frequency); stack.Push(new(SignatureTypeCode.Int64)); break;
                case SignatureTypeCode.Double: writer.F64Const(10_000_000d / StopwatchConstant.Frequency); stack.Push(new(SignatureTypeCode.Double)); break;
                default: writer.I32Const(1); stack.Push(new(SignatureTypeCode.Int32)); break;
            }
            return;
        }
        var identity = TimeSpanZero.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType);
        if (identity is null) { EmitStaticField(instruction, offset); return; }
        FrameAddress(Frame.Values[offset]);
        writer.I32Const(0).I32Const(Frame.ValueSize(identity.Value)).MemoryFill();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle, identity));
    }
}
