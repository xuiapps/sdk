using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdsflda(Instruction.Token instruction, int offset)
    {
        if (method.StaticSpans.ContainsKey(offset)) EmitLdtoken(instruction, offset);
        else EmitStaticField(instruction, offset);
    }
}
